using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Api.Modules;
using Deliveries.Domain.Ids;
using Deliveries.Infrastructure.Messaging.Consumers;
using Deliveries.Infrastructure.Persistence;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Messaging.Consumers;
using Ordering.Infrastructure.Persistence;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.Infrastructure.Interceptors;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.Messaging;

namespace FoodDelivery.IntegrationTest.Messaging;

[Collection("Database")]
public class ConsumerIdempotencyTests(MsSqlContainerFixture fixture)
{
    private IConfiguration Configuration => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString
        })
        .Build();

    private async Task<(ServiceProvider Provider, ITestHarness Harness)> StartOrderingHarnessAsync()
    {
        var services = new ServiceCollection();
        services.AddScoped<DomainEventPublishInterceptor>();
        services.AddOrderingModule(Configuration);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OrderingDbContext).Assembly));
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<OrderProcessConsumer>().Endpoint(e => e.Name = Queues.OrderProcess);
            x.AddConsumer<ConfirmOrderConsumer>().Endpoint(e => e.Name = Queues.ConfirmOrder);
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(10);
        await harness.Start();
        return (provider, harness);
    }

    private async Task<(ServiceProvider Provider, ITestHarness Harness)> StartDeliveriesHarnessAsync()
    {
        var services = new ServiceCollection();
        services.AddScoped<DomainEventPublishInterceptor>();
        services.AddDeliveriesModule(Configuration);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DeliveriesDbContext).Assembly));
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<CreateDeliveryConsumer>().Endpoint(e => e.Name = Queues.CreateDelivery);
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(10);
        await harness.Start();
        return (provider, harness);
    }

    private static Order CreatePendingOrder()
    {
        var order = Order.Create(new OrderId(Guid.CreateVersion7()), new RestaurantRefId(Guid.CreateVersion7()));
        order.AddOrderLineItem(new OrderLineId(Guid.CreateVersion7()), Money.Create(Currency.Usd, 12.5m).Ok!,
            new MenuItemRefId(Guid.CreateVersion7()));
        order.Place(Money.Create(Currency.Usd, 0m).Ok!);
        return order;
    }

    [Fact]
    public async Task OrderProcessConsumer_RedeliveredMessage_ShouldNotThrowOrChangeStateTwice()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartOrderingHarnessAsync();
        await using var _ = provider;

        var order = CreatePendingOrder();
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }
        
        await harness.Bus.Publish(new OrderProcess(order.Id.Id));
        await harness.Bus.Publish(new OrderProcess(order.Id.Id));

        (await harness.Consumed.SelectAsync<OrderProcess>().CountAsync()).Should().Be(2);

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var reloaded = await readContext.Orders.FirstAsync(o => o.Id == order.Id);
        reloaded.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task ConfirmOrderConsumer_ForAlreadyConfirmedOrder_ShouldLeaveStateUnchanged()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartOrderingHarnessAsync();
        await using var _ = provider;

        var order = CreatePendingOrder();
        order.StartProcessing();
        order.Confirm();
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        await harness.Bus.Publish(new ConfirmOrder(order.Id.Id));
        (await harness.Consumed.Any<ConfirmOrder>()).Should().BeTrue();

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var reloaded = await readContext.Orders.FirstAsync(o => o.Id == order.Id);
        reloaded.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task CreateDeliveryConsumer_RedeliveredMessage_ShouldBeIgnoredGracefully()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartDeliveriesHarnessAsync();
        await using var _ = provider;

        var orderId = Guid.CreateVersion7();

        await harness.Bus.Publish(new CreateDelivery(orderId));
        await harness.Bus.Publish(new CreateDelivery(orderId));

        (await harness.Consumed.SelectAsync<CreateDelivery>().CountAsync()).Should().Be(2);

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<DeliveriesDbContext>();
        var deliveries = await readContext.Deliveries
            .Where(d => d.OrderRefId == new OrderRefId(orderId))
            .ToListAsync();
        
        deliveries.Should().ContainSingle();
        (await harness.Published.Any<Fault<CreateDelivery>>()).Should().BeFalse();
    }
}
