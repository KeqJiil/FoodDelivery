using FluentAssertions;
using MassTransit.Testing;
using Saga.Application;
using Saga.UnitTest.TestHelpers;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace Saga.UnitTest.Application;

public class OrderSagaTimeoutTests : OrderSagaTestBase
{
    [Fact]
    public async Task ApprovalTimeout_ShouldCancelTheOrderRequest()
    {
        var orderId = Guid.NewGuid();
        var instance = await GivenAwaitingApproval(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), instance.ApprovalTimeoutTokenId);
        var instanceId = await SagaHarness.Exists(orderId, m => m.CompensatingRequest, Harness.TestTimeout);

        instanceId.Should().NotBeNull();
        (await WasSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
        Instance(orderId).FailedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ApprovalTimeout_ShouldNotFailTheOrderDirectly()
    {
        var orderId = Guid.NewGuid();
        var instance = await GivenAwaitingApproval(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), instance.ApprovalTimeoutTokenId);
        await SagaHarness.Exists(orderId, m => m.CompensatingRequest, Harness.TestTimeout);

        (await NothingSent<OrderFail>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task ApprovalTimeoutChain_ShouldEndWithAFinalizedSaga()
    {
        var orderId = Guid.NewGuid();
        var instance = await GivenAwaitingApproval(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), instance.ApprovalTimeoutTokenId);
        await SagaHarness.Exists(orderId, m => m.CompensatingRequest, Harness.TestTimeout);
        await Advance(orderId, new OrderRequestCancelledIntegration(orderId), m => m.CompensatingOrder);
        await Harness.Bus.Publish(new OrderFailedIntegration(orderId));

        (await SagaHarness.NotExists(orderId, Harness.TestTimeout)).Should().BeNull();
    }

    [Fact]
    public async Task PaymentTimeout_ShouldCancelThePayment()
    {
        var orderId = Guid.NewGuid();
        var instance = await GivenAwaitingPayment(orderId);

        await FireTimeout(new PaymentTimeoutExpired(orderId), instance.PaymentTimeoutTokenId);
        var instanceId = await SagaHarness.Exists(orderId, m => m.CompensatingPayment, Harness.TestTimeout);

        instanceId.Should().NotBeNull();
        (await WasSent<CancelPayment>(m => m.OrderId == orderId)).Should().BeTrue();
        Instance(orderId).FailedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PaymentTimeout_ShouldNotCancelTheRequestBeforeThePayment()
    {
        // Cancelling the request while money may still be captured would strand the payment.
        var orderId = Guid.NewGuid();
        var instance = await GivenAwaitingPayment(orderId);

        await FireTimeout(new PaymentTimeoutExpired(orderId), instance.PaymentTimeoutTokenId);
        await SagaHarness.Exists(orderId, m => m.CompensatingPayment, Harness.TestTimeout);

        (await NothingSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task StaleApprovalTimeout_WhileAwaitingProcessing_ShouldNotDerailTheFlow()
    {
        var orderId = Guid.NewGuid();
        var instance = await GivenAwaitingProcessing(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), instance.ApprovalTimeoutTokenId);
        await Advance(orderId, new OrderStartedProcessingIntegration(orderId), m => m.AwaitingPayment);

        (await NothingSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task StaleApprovalTimeout_WhileAwaitingPayment_ShouldNotDerailTheFlow()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingPayment(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), null);
        await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);

        (await NothingSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task StaleTimeouts_WhileAwaitingConfirmation_ShouldNotDerailTheFlow()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingConfirmation(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), null);
        await FireTimeout(new PaymentTimeoutExpired(orderId), null);
        await Advance(orderId, new OrderConfirmedIntegration(orderId), m => m.AwaitingDelivery);

        (await NothingSent<CancelPayment>(m => m.OrderId == orderId)).Should().BeTrue();
        (await NothingSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task StaleTimeouts_WhileAwaitingDelivery_ShouldNotBlockCompletion()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingDelivery(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), null);
        await FireTimeout(new PaymentTimeoutExpired(orderId), null);
        await Harness.Bus.Publish(new DeliveryPlacedIntegration(orderId));

        (await SagaHarness.NotExists(orderId, Harness.TestTimeout)).Should().BeNull();
    }

    [Fact]
    public async Task StaleTimeouts_WhileCompensatingRequest_ShouldNotDerailCompensation()
    {
        var orderId = Guid.NewGuid();
        await GivenCompensatingRequest(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), null);
        await FireTimeout(new PaymentTimeoutExpired(orderId), null);
        await Advance(orderId, new OrderRequestCancelledIntegration(orderId), m => m.CompensatingOrder);

        (await WasSent<OrderFail>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task StaleTimeouts_WhileCompensatingOrder_ShouldNotDerailCompensation()
    {
        var orderId = Guid.NewGuid();
        await GivenCompensatingOrder(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), null);
        await FireTimeout(new PaymentTimeoutExpired(orderId), null);
        await Harness.Bus.Publish(new OrderFailedIntegration(orderId));

        (await SagaHarness.NotExists(orderId, Harness.TestTimeout)).Should().BeNull();
    }

    [Fact]
    public async Task StaleApprovalTimeout_WhileCompensatingPayment_ShouldNotDerailCompensation()
    {
        var orderId = Guid.NewGuid();
        await GivenCompensatingPayment(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), null);
        await Advance(orderId, new PaymentCancelledIntegration(orderId), m => m.CompensatingRequest);

        (await WasSent<CancelOrderRequest>(m => m.OrderId == orderId)).Should().BeTrue();
    }
}
