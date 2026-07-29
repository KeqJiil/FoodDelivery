using FluentAssertions;
using MassTransit.Testing;
using Saga.UnitTest.TestHelpers;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace Saga.UnitTest.Application;

public class OrderSagaCompensationTests : OrderSagaTestBase
{
    [Fact]
    public async Task OrderRejected_ShouldFailTheOrder_AndStampFailedAt()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingApproval(orderId);

        var instance = await Advance(orderId, new OrderRejectedIntegration(orderId), m => m.CompensatingOrder);

        (await WasSent<OrderFail>(m => m.OrderId == orderId)).Should().BeTrue();
        instance.FailedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task OrderRejected_ShouldCancelTheApprovalTimeout()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingApproval(orderId);

        var instance = await Advance(orderId, new OrderRejectedIntegration(orderId), m => m.CompensatingOrder);

        instance.ApprovalTimeoutTokenId.Should().BeNull();
    }

    [Fact]
    public async Task OrderRejected_ShouldNotTouchTheOrderRequest()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingApproval(orderId);

        await Advance(orderId, new OrderRejectedIntegration(orderId), m => m.CompensatingOrder);

        (await NothingSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task OrderCancelled_WhileAwaitingApproval_ShouldCancelTheRequestFirst()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingApproval(orderId);

        var instance = await Advance(orderId, new OrderCancelledIntegration(orderId), m => m.CompensatingRequest);

        (await WasSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
        instance.FailedAt.Should().NotBeNull();
        instance.ApprovalTimeoutTokenId.Should().BeNull();
    }

    [Fact]
    public async Task PaymentFailed_ShouldCompensateTheRequest_AndCancelPaymentTimeout()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingPayment(orderId);

        var instance = await Advance(orderId, new PaymentFailedIntegration(orderId, "card declined"),
            m => m.CompensatingRequest);

        (await WasSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
        instance.FailedAt.Should().NotBeNull();
        instance.PaymentTimeoutTokenId.Should().BeNull();
    }

    [Fact]
    public async Task OrderRequestCancelled_ShouldMoveOnToFailingTheOrder()
    {
        var orderId = Guid.NewGuid();
        await GivenCompensatingRequest(orderId);

        await Advance(orderId, new OrderRequestCancelledIntegration(orderId), m => m.CompensatingOrder);

        (await WasSent<OrderFail>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task OrderFailed_ShouldFinalizeTheSaga()
    {
        var orderId = Guid.NewGuid();
        await GivenCompensatingOrder(orderId);

        await Harness.Bus.Publish(new OrderFailedIntegration(orderId));
        (await SagaHarness.Consumed.Any<OrderFailedIntegration>(x => x.Context.Message.Id == orderId))
            .Should().BeTrue();

        (await SagaHarness.NotExists(orderId, Harness.TestTimeout)).Should().BeNull();
    }

    [Fact]
    public async Task RejectionChain_ShouldRunEndToEnd()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingApproval(orderId);

        await Advance(orderId, new OrderRejectedIntegration(orderId), m => m.CompensatingOrder);
        await Harness.Bus.Publish(new OrderFailedIntegration(orderId));

        (await SagaHarness.NotExists(orderId, Harness.TestTimeout)).Should().BeNull();
        (await WasSent<OrderFail>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task PaymentFailureChain_ShouldCompensateRequestThenOrder()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingPayment(orderId);

        await Advance(orderId, new PaymentFailedIntegration(orderId, "insufficient funds"),
            m => m.CompensatingRequest);
        await Advance(orderId, new OrderRequestCancelledIntegration(orderId), m => m.CompensatingOrder);
        await Harness.Bus.Publish(new OrderFailedIntegration(orderId));

        (await SagaHarness.NotExists(orderId, Harness.TestTimeout)).Should().BeNull();
        (await WasSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
        (await WasSent<OrderFail>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task PaymentCancelled_ShouldContinueCompensatingTheRequest()
    {
        var orderId = Guid.NewGuid();
        await GivenCompensatingPayment(orderId);

        await Advance(orderId, new PaymentCancelledIntegration(orderId), m => m.CompensatingRequest);

        (await WasSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task PaymentSucceeded_AfterPaymentTimeout_ShouldResumeTheHappyPath()
    {
        var orderId = Guid.NewGuid();
        await GivenCompensatingPayment(orderId);

        await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);

        (await WasSent<ConfirmOrder>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task RecoveredPayment_ShouldStillCompleteDelivery()
    {
        var orderId = Guid.NewGuid();
        await GivenCompensatingPayment(orderId);
        await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);

        await Advance(orderId, new OrderConfirmedIntegration(orderId), m => m.AwaitingDelivery);
        await Harness.Bus.Publish(new DeliveryPlacedIntegration(orderId));

        (await SagaHarness.NotExists(orderId, Harness.TestTimeout)).Should().BeNull();
        (await WasSent<CreateDelivery>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task RecoveredPayment_ShouldClearTheFailedAtStamp()
    {
        var orderId = Guid.NewGuid();
        var compensating = await GivenCompensatingPayment(orderId);
        compensating.FailedAt.Should().NotBeNull();

        var instance = await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);

        instance.FailedAt.Should().BeNull();
    }
}
