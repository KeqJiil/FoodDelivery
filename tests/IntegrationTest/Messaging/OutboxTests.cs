using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Persistence;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.Infrastructure.Interceptors;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace FoodDelivery.IntegrationTest.Messaging;

public class OrderPlacedIntegrationSubscriber : IConsumer<OrderPlacedIntegration>
{
    public Task Consume(ConsumeContext<OrderPlacedIntegration> context)
    {
        return Task.CompletedTask;
    }
}

[Collection("Database")]
public class OutboxTests(MsSqlContainerFixture fixture) : IAsyncDisposable
{
    private IHost? _host;

    public async ValueTask DisposeAsync()
    {
        if (_host is null) return;
        await _host.StopAsync();
        _host.Dispose();
    }

    private async Task<ITestHarness> StartOrderingOutboxHostAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString
            })
            .Build();

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddScoped<DomainEventPublishInterceptor>();
        builder.Services.AddScoped<IIntegrationEventTranslator<Ordering.Domain.Events.OrderPlaced>,
            Ordering.Infrastructure.Messaging.Translators.OrderPlacedIntegrationEventTranslator>();
        builder.Services.AddDbContext<OrderingDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "ordering").EnableRetryOnFailure());
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });
        builder.Services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<OrderPlacedIntegrationSubscriber>();
            x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
            {
                o.UseSqlServer();
                o.QueryDelay = TimeSpan.FromMilliseconds(200);
                o.UseBusOutbox();
            });
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });

        _host = builder.Build();
        await _host.StartAsync();

        var harness = _host.Services.GetRequiredService<ITestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(10);
        harness.TestInactivityTimeout = TimeSpan.FromSeconds(10);
        await harness.Start();
        return harness;
    }

    private static Order CreatePlaceableOrder()
    {
        var order = Order.Create(new OrderId(Guid.CreateVersion7()), new RestaurantRefId(Guid.CreateVersion7()));
        order.AddOrderLineItem(new OrderLineId(Guid.CreateVersion7()), Money.Create(Currency.Usd, 12.5m).Ok!,
            new MenuItemRefId(Guid.CreateVersion7()));
        return order;
    }

    [Fact]
    public async Task CommittedTransaction_ShouldEventuallyPublishExactlyOnce()
    {
        await fixture.ResetDatabaseAsync();
        var harness = await StartOrderingOutboxHostAsync();

        var order = CreatePlaceableOrder();
        order.Place(Money.Create(Currency.Usd, 0m).Ok!);

        using (var scope = _host!.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        using var cts = new CancellationTokenSource(harness.TestTimeout);
        (await harness.Consumed.Any<OrderPlacedIntegration>(m => m.Context.Message.OrderId == order.Id.Id,
                cts.Token))
            .Should().BeTrue();
        (await harness.Consumed.SelectAsync<OrderPlacedIntegration>()
                .Where(m => m.Context.Message.OrderId == order.Id.Id).CountAsync())
            .Should().Be(1);
    }

    [Fact]
    public async Task RolledBackTransaction_ShouldNeverPublish()
    {
        await fixture.ResetDatabaseAsync();
        var harness = await StartOrderingOutboxHostAsync();

        var order = CreatePlaceableOrder();
        order.Place(Money.Create(Currency.Usd, 0m).Ok!);

        using (var scope = _host!.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();
                context.Orders.Add(order);
                await context.SaveChangesAsync();
                await transaction.RollbackAsync();
            });
        }

        await Task.Delay(1500);
        (await harness.Consumed.Any<OrderPlacedIntegration>(m => m.Context.Message.OrderId == order.Id.Id))
            .Should().BeFalse();

        using var readScope = _host!.Services.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        (await readContext.Orders.AnyAsync(o => o.Id == order.Id)).Should().BeFalse();
    }
}