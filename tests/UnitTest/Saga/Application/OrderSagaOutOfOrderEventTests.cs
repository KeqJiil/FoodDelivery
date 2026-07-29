using FluentAssertions;
using MassTransit.Testing;
using Saga.UnitTest.TestHelpers;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace Saga.UnitTest.Application;

public class OrderSagaOutOfOrderEventTests : OrderSagaTestBase
{
    [Fact]
    public async Task LateCancellation_WhileAwaitingProcessing_ShouldBeIgnored()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingProcessing(orderId);

        await Harness.Bus.Publish(new OrderCancelledIntegration(orderId));
        await Advance(orderId, new OrderStartedProcessingIntegration(orderId), m => m.AwaitingPayment);

        (await NothingSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
        (await NothingSent<OrderFail>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task LateCancellation_WhileAwaitingPayment_ShouldBeIgnored()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingPayment(orderId);

        await Harness.Bus.Publish(new OrderCancelledIntegration(orderId));
        await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);

        (await NothingSent<CancelPayment>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task LateCancellation_WhileAwaitingConfirmation_ShouldBeIgnored()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingConfirmation(orderId);

        await Harness.Bus.Publish(new OrderCancelledIntegration(orderId));
        await Advance(orderId, new OrderConfirmedIntegration(orderId), m => m.AwaitingDelivery);

        (await NothingSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task LateCancellation_WhileAwaitingDelivery_ShouldNotBlockCompletion()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingDelivery(orderId);

        await Harness.Bus.Publish(new OrderCancelledIntegration(orderId));
        await Harness.Bus.Publish(new DeliveryPlacedIntegration(orderId));

        (await SagaHarness.NotExists(orderId, Harness.TestTimeout)).Should().BeNull();
        (await NothingSent<OrderFail>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task LateCancellation_ShouldNotStampFailedAt_OnAnOrderInFlight()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingPayment(orderId);

        await Harness.Bus.Publish(new OrderCancelledIntegration(orderId));
        var instance = await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);

        instance.FailedAt.Should().BeNull();
    }

    [Fact]
    public async Task OrderApproved_ForAnUnknownOrder_ShouldNotCreateAnInstance()
    {
        var unknownId = Guid.NewGuid();

        await Harness.Bus.Publish(new OrderApprovedIntegration(unknownId));
        await Harness.InactivityTask;

        SagaHarness.Sagas.Contains(unknownId).Should().BeNull();
    }

    [Fact]
    public async Task PaymentSucceeded_ForAnUnknownOrder_ShouldNotCreateAnInstance()
    {
        var unknownId = Guid.NewGuid();

        await Harness.Bus.Publish(new PaymentSucceededIntegration(unknownId));
        await Harness.InactivityTask;

        SagaHarness.Sagas.Contains(unknownId).Should().BeNull();
    }

    [Fact]
    public async Task DeliveryPlaced_ForAnUnknownOrder_ShouldNotCreateAnInstance()
    {
        var unknownId = Guid.NewGuid();

        await Harness.Bus.Publish(new DeliveryPlacedIntegration(unknownId));
        await Harness.InactivityTask;

        SagaHarness.Sagas.Contains(unknownId).Should().BeNull();
    }

    [Fact]
    public async Task OrderCancelled_ForAnUnknownOrder_ShouldNotCreateAnInstance()
    {
        var unknownId = Guid.NewGuid();

        await Harness.Bus.Publish(new OrderCancelledIntegration(unknownId));
        await Harness.InactivityTask;

        SagaHarness.Sagas.Contains(unknownId).Should().BeNull();
        (await NothingSent<CancelOrderRequest>(m => m.OrderId == unknownId)).Should().BeTrue();
    }

    [Fact]
    public async Task OnlyOrderPlaced_ShouldStartASaga()
    {
        var orderId = Guid.NewGuid();

        await Harness.Bus.Publish(new OrderStartedProcessingIntegration(orderId));
        await Harness.InactivityTask;
        SagaHarness.Sagas.Contains(orderId).Should().BeNull();

        await GivenAwaitingApproval(orderId);

        SagaHarness.Sagas.Contains(orderId).Should().NotBeNull();
    }
}
