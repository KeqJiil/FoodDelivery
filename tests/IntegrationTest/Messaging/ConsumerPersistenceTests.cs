using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Api.Modules;
using Deliveries.Domain.Enums;
using Deliveries.Infrastructure.Messaging.Consumers;
using Deliveries.Infrastructure.Persistence;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Messaging.Consumers;
using Ordering.Infrastructure.Persistence;
using OrderRequests.Domain.Enums;
using OrderRequests.Infrastructure.Messaging.Consumers;
using OrderRequests.Infrastructure.Persistence;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Payments.Infrastructure.Messaging.Consumers;
using Payments.Infrastructure.Persistence;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.Infrastructure.Interceptors;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Messaging;
using OrderRequestAggregate = OrderRequests.Domain.Aggregates.OrderRequest;
using DeliveriesOrderRefId = Deliveries.Domain.Ids.OrderRefId;
using PaymentsOrderRefId = Payments.Domain.Ids.OrderRefId;
using OrderRequestsOrderRefId = OrderRequests.Domain.Ids.OrderRefId;

namespace FoodDelivery.IntegrationTest.Messaging;

[Collection("Database")]
public class ConsumerPersistenceTests(MsSqlContainerFixture fixture)
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
            x.AddConsumer<OrderFailConsumer>().Endpoint(e => e.Name = Queues.OrderFail);
            x.AddConsumer<ConfirmOrderConsumer>().Endpoint(e => e.Name = Queues.ConfirmOrder);
            x.AddConsumer<ChangeOrderLineItemPriceConsumer>()
                .Endpoint(e => e.Name = Queues.MenuItemPriceChanged);
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

    private async Task<(ServiceProvider Provider, ITestHarness Harness)> StartPaymentsHarnessAsync()
    {
        var services = new ServiceCollection();
        services.AddScoped<DomainEventPublishInterceptor>();
        services.AddPaymentsModule(Configuration);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PaymentsDbContext).Assembly));
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<OrderConfirmedConsumer>().Endpoint(e => e.Name = Queues.CreatePayment);
            x.AddConsumer<CancelPaymentConsumer>().Endpoint(e => e.Name = Queues.CancelPayment);
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(10);
        await harness.Start();
        return (provider, harness);
    }

    private async Task<(ServiceProvider Provider, ITestHarness Harness)> StartOrderRequestsHarnessAsync()
    {
        var services = new ServiceCollection();
        services.AddScoped<DomainEventPublishInterceptor>();
        services.AddOrderRequestsModule(Configuration);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OrderRequestsDbContext).Assembly));
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<OrderRequestedConsumer>().Endpoint(e => e.Name = Queues.CreateRequest);
            x.AddConsumer<OrderCancelConsumer>().Endpoint(e => e.Name = Queues.CancelOrderRequest);
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
    public async Task OrderProcessConsumer_ShouldMoveOrderToProcessing()
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
        (await harness.Consumed.Any<OrderProcess>()).Should().BeTrue();

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var reloaded = await readContext.Orders.FirstAsync(o => o.Id == order.Id);
        reloaded.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task ConfirmOrderConsumer_ShouldMoveOrderToConfirmed()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartOrderingHarnessAsync();
        await using var _ = provider;

        var order = CreatePendingOrder();
        order.StartProcessing();
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
    public async Task OrderFailConsumer_ShouldMoveOrderToFailed()
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

        await harness.Bus.Publish(new OrderFail(order.Id.Id));
        (await harness.Consumed.Any<OrderFail>()).Should().BeTrue();

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var reloaded = await readContext.Orders.FirstAsync(o => o.Id == order.Id);
        reloaded.Status.Should().Be(OrderStatus.Failed);
    }

    [Fact]
    public async Task ChangeOrderLineItemPriceConsumer_ShouldUpdateDraftOrderLinePrice()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartOrderingHarnessAsync();
        await using var _ = provider;

        var menuItemId = new MenuItemRefId(Guid.CreateVersion7());
        var order = Order.Create(new OrderId(Guid.CreateVersion7()), new RestaurantRefId(Guid.CreateVersion7()));
        order.AddOrderLineItem(new OrderLineId(Guid.CreateVersion7()), Money.Create(Currency.Usd, 12.5m).Ok!,
            menuItemId);
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        await harness.Bus.Publish(new MenuItemPriceChangedIntegration(menuItemId.Id, 20m, Currency.Usd));
        (await harness.Consumed.Any<MenuItemPriceChangedIntegration>()).Should().BeTrue();

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var reloaded = await readContext.Orders.Include(o => o.OrderLines).FirstAsync(o => o.Id == order.Id);
        reloaded.OrderLines.Single().Price.Should().Be(Money.Create(Currency.Usd, 20m).Ok);
    }

    [Fact]
    public async Task CreateDeliveryConsumer_ShouldCreatePendingDelivery()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartDeliveriesHarnessAsync();
        await using var _ = provider;

        var orderId = Guid.CreateVersion7();

        await harness.Bus.Publish(new CreateDelivery(orderId));
        (await harness.Consumed.Any<CreateDelivery>()).Should().BeTrue();

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<DeliveriesDbContext>();
        var delivery = await readContext.Deliveries.FirstAsync(d => d.OrderRefId == new DeliveriesOrderRefId(orderId));
        delivery.Status.Should().Be(DeliveryStatus.Pending);
    }

    [Fact]
    public async Task OrderConfirmedConsumer_ShouldCreatePayment()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartPaymentsHarnessAsync();
        await using var _ = provider;

        var orderId = Guid.CreateVersion7();
        
        await harness.Bus.Publish(new CreatePayment(orderId, 15.5m, Currency.Eur));
        (await harness.Consumed.Any<CreatePayment>()).Should().BeTrue();

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var payment = await readContext.Payments.FirstAsync(p => p.OrderRefId == new PaymentsOrderRefId(orderId));
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.Amount.Should().Be(Money.Create(Currency.Eur, 15.5m).Ok);
    }

    [Fact]
    public async Task CancelPaymentConsumer_ShouldCancelPendingPayment()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartPaymentsHarnessAsync();
        await using var _ = provider;

        var orderRefId = new PaymentsOrderRefId(Guid.CreateVersion7());
        var payment = Payment.Create(new Payments.Domain.Ids.PaymentId(Guid.CreateVersion7()), orderRefId,
            Money.Create(Currency.Eur, 15.5m).Ok!);
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            context.Payments.Add(payment);
            await context.SaveChangesAsync();
        }

        await harness.Bus.Publish(new CancelPayment(orderRefId.Id));
        (await harness.Consumed.Any<CancelPayment>()).Should().BeTrue();

        (await harness.Published.Any<PaymentCancelledIntegration>(m => m.Context.Message.OrderId == orderRefId.Id))
            .Should().BeTrue();

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var reloaded = await readContext.Payments.FirstAsync(p => p.Id == payment.Id);
        reloaded.Status.Should().Be(PaymentStatus.Cancelled);
    }

    [Fact]
    public async Task OrderRequestedConsumer_ShouldCreatePendingOrderRequest()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartOrderRequestsHarnessAsync();
        await using var _ = provider;

        var orderId = Guid.CreateVersion7();
        var restaurantId = Guid.CreateVersion7();

        await harness.Bus.Publish(new CreateRequest(orderId, restaurantId));
        (await harness.Consumed.Any<CreateRequest>()).Should().BeTrue();

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<OrderRequestsDbContext>();
        var orderRequest =
            await readContext.OrderRequests.FirstAsync(r => r.OrderRefId == new OrderRequestsOrderRefId(orderId));
        orderRequest.Status.Should().Be(OrderRequestStatus.Pending);
    }

    [Fact]
    public async Task OrderCancelConsumer_ShouldCancelPendingOrderRequest()
    {
        await fixture.ResetDatabaseAsync();
        var (provider, harness) = await StartOrderRequestsHarnessAsync();
        await using var _ = provider;

        var orderRequest = OrderRequestAggregate.Create(new OrderRequests.Domain.Ids.OrderRequestId(Guid.CreateVersion7()),
            new OrderRequestsOrderRefId(Guid.CreateVersion7()),
            new OrderRequests.Domain.Ids.RestaurantRefId(Guid.CreateVersion7()));
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OrderRequestsDbContext>();
            context.OrderRequests.Add(orderRequest);
            await context.SaveChangesAsync();
        }

        await harness.Bus.Publish(new CancelOrderRequest(orderRequest.OrderRefId.Id));
        (await harness.Consumed.Any<CancelOrderRequest>()).Should().BeTrue();

        using var readScope = provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<OrderRequestsDbContext>();
        var reloaded = await readContext.OrderRequests.FirstAsync(r => r.Id == orderRequest.Id);
        reloaded.Status.Should().Be(OrderRequestStatus.Cancelled);
    }
}
