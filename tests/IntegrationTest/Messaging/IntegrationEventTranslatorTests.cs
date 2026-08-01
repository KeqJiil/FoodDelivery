using FluentAssertions;
using Deliveries.Domain.Events;
using Deliveries.Infrastructure.Messaging.Translators;
using Ordering.Domain.Events;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Messaging.Translators;
using OrderRequests.Infrastructure.Messaging.Translators;
using Payments.Domain.Events;
using Payments.Infrastructure.Messaging.Translators;
using Restaurants.Domain.Events;
using Restaurants.Infrastructure.Messaging.Translators;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace FoodDelivery.IntegrationTest.Messaging;

public class IntegrationEventTranslatorTests
{
    [Fact]
    public void OrderPlacedTranslator_ShouldMapToIntegrationEvent()
    {
        var orderId = new OrderId(Guid.CreateVersion7());
        var restaurantId = new RestaurantRefId(Guid.CreateVersion7());
        var price = Money.Create(Currency.Eur, 42.5m).Ok!;
        var domainEvent = new OrderPlaced(orderId, restaurantId, price);

        var result = new OrderPlacedIntegrationEventTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new OrderPlacedIntegration(orderId.Id, restaurantId.Id, 42.5m, Currency.Eur), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void OrderConfirmedTranslator_ShouldMapToIntegrationEvent()
    {
        var orderId = new OrderId(Guid.CreateVersion7());
        var domainEvent = new OrderConfirmed(orderId);

        var result = new OrderConfirmedIntegrationEventTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new OrderConfirmedIntegration(orderId.Id), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void OrderCancelledTranslator_ShouldMapToIntegrationEvent()
    {
        var orderId = new OrderId(Guid.CreateVersion7());
        var domainEvent = new OrderCancelled(orderId);

        var result = new OrderCancelledIntegrationEventTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new OrderCancelledIntegration(orderId.Id), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void OrderFailedTranslator_ShouldMapToIntegrationEvent()
    {
        var orderId = new OrderId(Guid.CreateVersion7());
        var domainEvent = new OrderFailed(orderId);

        var result = new OrderFailedIntegrationTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new OrderFailedIntegration(orderId.Id), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void OrderStartedProcessingTranslator_ShouldMapToIntegrationEvent()
    {
        var orderId = new OrderId(Guid.CreateVersion7());
        var domainEvent = new OrderStartedProcessing(orderId);

        var result = new OrderStartedProcessingIntegrationTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new OrderStartedProcessingIntegration(orderId.Id), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void OrderApprovedTranslator_ShouldMapToIntegrationEvent()
    {
        var orderRequestId = new OrderRequests.Domain.Ids.OrderRequestId(Guid.CreateVersion7());
        var orderRefId = new OrderRequests.Domain.Ids.OrderRefId(Guid.CreateVersion7());
        var domainEvent = new OrderRequests.Domain.Events.OrderApproved(orderRequestId, orderRefId);

        var result = new OrderApprovedIntegrationEventTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new OrderApprovedIntegration(orderRefId.Id), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void OrderRequestCancelledTranslator_ShouldMapToIntegrationEvent()
    {
        var orderRequestId = new OrderRequests.Domain.Ids.OrderRequestId(Guid.CreateVersion7());
        var domainEvent = new OrderRequests.Domain.Events.OrderCancelled(orderRequestId);

        var result = new OrderRequestCancelledIntegrationTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new OrderRequestCancelledIntegration(orderRequestId.Id), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void PaymentSucceededTranslator_ShouldMapToIntegrationEvent()
    {
        var paymentId = new Payments.Domain.Ids.PaymentId(Guid.CreateVersion7());
        var orderRefId = new Payments.Domain.Ids.OrderRefId(Guid.CreateVersion7());
        var domainEvent = new PaymentSucceeded(paymentId, orderRefId);

        var result = new PaymentSucceededIntegrationEventTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new PaymentSucceededIntegration(orderRefId.Id), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void PaymentFailedTranslator_ShouldMapToIntegrationEvent()
    {
        var paymentId = new Payments.Domain.Ids.PaymentId(Guid.CreateVersion7());
        var orderRefId = new Payments.Domain.Ids.OrderRefId(Guid.CreateVersion7());
        var domainEvent = new PaymentFailed(paymentId, orderRefId, "gateway declined");

        var result = new PaymentFailedIntegrationEventTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new PaymentFailedIntegration(orderRefId.Id, "gateway declined"), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void PaymentCancelledTranslator_ShouldMapToIntegrationEvent()
    {
        var paymentId = new Payments.Domain.Ids.PaymentId(Guid.CreateVersion7());
        var orderRefId = new Payments.Domain.Ids.OrderRefId(Guid.CreateVersion7());
        var domainEvent = new PaymentCancelled(paymentId, orderRefId);

        var result = new PaymentCancelledIntegrationTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new PaymentCancelledIntegration(orderRefId.Id), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void DeliveryPlacedTranslator_ShouldMapToIntegrationEvent()
    {
        var deliveryId = new Deliveries.Domain.Ids.DeliveryId(Guid.CreateVersion7());
        var orderRefId = new Deliveries.Domain.Ids.OrderRefId(Guid.CreateVersion7());
        var domainEvent = new DeliveryCreated(deliveryId, orderRefId);

        var result = new DeliveryPlacedTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new DeliveryPlacedIntegration(orderRefId.Id), opts => opts.Excluding(e => e.OccurredOn));
    }

    [Fact]
    public void MenuItemPriceChangedTranslator_ShouldMapToIntegrationEvent()
    {
        var restaurantId = new Restaurants.Domain.Ids.RestaurantId(Guid.CreateVersion7());
        var menuItemId = new Restaurants.Domain.Ids.MenuItemId(Guid.CreateVersion7());
        var newPrice = Money.Create(Currency.Usd, 9.99m).Ok!;
        var domainEvent = new MenuItemPriceChanged(restaurantId, menuItemId, newPrice);

        var result = new MenuItemPriceChangedIntegrationEventTranslator().Translate(domainEvent);

        result.Should().BeEquivalentTo(new MenuItemPriceChangedIntegration(menuItemId.Id, 9.99m, Currency.Usd), opts => opts.Excluding(e => e.OccurredOn));
    }
}
