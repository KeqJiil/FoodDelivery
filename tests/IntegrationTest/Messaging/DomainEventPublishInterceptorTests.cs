using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Events;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Messaging.Translators;
using Ordering.Infrastructure.Persistence;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.Infrastructure.Interceptors;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace FoodDelivery.IntegrationTest.Messaging;

[Collection("Database")]
public class DomainEventPublishInterceptorTests(MsSqlContainerFixture fixture) : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();

        var services = new ServiceCollection();
        services.AddScoped<DomainEventPublishInterceptor>();
        services.AddScoped<IIntegrationEventTranslator<OrderPlaced>, OrderPlacedIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<OrderConfirmed>, OrderConfirmedIntegrationEventTranslator>();

        services.AddDbContext<OrderingDbContext>((sp, options) =>
        {
            options.UseSqlServer(fixture.ConnectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "ordering"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });

        services.AddMassTransitTestHarness(x => x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context)));

        _provider = services.BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
        _harness.TestTimeout = TimeSpan.FromSeconds(10);
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    private static Order CreatePlaceableOrder()
    {
        var order = Order.Create(new OrderId(Guid.CreateVersion7()), new RestaurantRefId(Guid.CreateVersion7()));
        order.AddOrderLineItem(new OrderLineId(Guid.CreateVersion7()), Money.Create(Currency.Usd, 12.5m).Ok!,
            new MenuItemRefId(Guid.CreateVersion7()));
        return order;
    }

    [Fact]
    public async Task SaveChanges_ShouldPublishIntegrationEvent_ForDomainEventWithTranslator()
    {
        var order = CreatePlaceableOrder();
        order.Place(Money.Create(Currency.Usd, 0m).Ok!);

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        (await _harness.Published.Any<OrderPlacedIntegration>(m => m.Context.Message.OrderId == order.Id.Id))
            .Should().BeTrue();
    }

    [Fact]
    public async Task SaveChanges_ShouldNotRepublish_OnSecondSaveChangesWithNoNewEvents()
    {
        var order = CreatePlaceableOrder();
        order.Place(Money.Create(Currency.Usd, 0m).Ok!);

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await context.SaveChangesAsync();

        _harness.Published.Select<OrderPlacedIntegration>(m => m.Context.Message.OrderId == order.Id.Id)
            .Should().ContainSingle();
    }

    [Fact]
    public async Task SaveChanges_ShouldNotThrowOrPublish_WhenNoTranslatorIsRegisteredForTheDomainEvent()
    {
        var order = CreatePlaceableOrder();
        order.Place(Money.Create(Currency.Usd, 0m).Ok!);

        using (var placeScope = _provider.CreateScope())
        {
            var placeContext = placeScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            placeContext.Orders.Add(order);
            await placeContext.SaveChangesAsync();
        }

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var reloaded = await context.Orders.FirstAsync(o => o.Id == order.Id);
        var failResult = reloaded.Fail();

        var act = async () => await context.SaveChangesAsync();

        await act.Should().NotThrowAsync();
        failResult.IsSuccess.Should().BeTrue();
        (await _harness.Published.Any<OrderFailedIntegration>(m => m.Context.Message.Id == order.Id.Id))
            .Should().BeFalse();
    }

    [Fact]
    public async Task SaveChanges_ShouldPublishForEveryAggregate_WhenMultipleAggregatesChangeInOneSaveChanges()
    {
        var firstOrder = CreatePlaceableOrder();
        var secondOrder = CreatePlaceableOrder();
        firstOrder.Place(Money.Create(Currency.Usd, 0m).Ok!);
        secondOrder.Place(Money.Create(Currency.Usd, 0m).Ok!);

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        context.Orders.AddRange(firstOrder, secondOrder);
        await context.SaveChangesAsync();

        (await _harness.Published.Any<OrderPlacedIntegration>(m => m.Context.Message.OrderId == firstOrder.Id.Id))
            .Should().BeTrue();
        (await _harness.Published.Any<OrderPlacedIntegration>(m => m.Context.Message.OrderId == secondOrder.Id.Id))
            .Should().BeTrue();
    }
}
