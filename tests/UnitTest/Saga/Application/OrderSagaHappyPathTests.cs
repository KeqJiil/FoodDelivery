using FluentAssertions;
using MassTransit.Testing;
using Saga.UnitTest.TestHelpers;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace Saga.UnitTest.Application;

public class OrderSagaHappyPathTests : OrderSagaTestBase
{
    [Fact]
    public async Task OrderPlaced_ShouldCreateInstance_AndRequestRestaurantApproval()
    {
        var orderId = Guid.NewGuid();
        var placed = Placed(orderId, 75.25m, Currency.Eur);

        await Advance(orderId, placed, m => m.AwaitingApproval);

        (await WasSent<CreateRequest>(m => m.OrderId == orderId && m.RestaurantId == placed.RestaurantId))
            .Should().BeTrue();
    }

    [Fact]
    public async Task OrderPlaced_ShouldCaptureAmountAndCurrency_OnTheInstance()
    {
        var orderId = Guid.NewGuid();

        var instance = await GivenAwaitingApproval(orderId, 123.45m, Currency.Gbp);

        instance.Amount.Should().Be(123.45m);
        instance.Currency.Should().Be(Currency.Gbp);
        instance.CorrelationId.Should().Be(orderId);
        instance.FailedAt.Should().BeNull();
    }

    [Fact]
    public async Task OrderPlaced_ShouldScheduleApprovalTimeout()
    {
        var orderId = Guid.NewGuid();

        var instance = await GivenAwaitingApproval(orderId);

        instance.ApprovalTimeoutTokenId.Should().NotBeNull();
        instance.PaymentTimeoutTokenId.Should().BeNull();
    }

    [Fact]
    public async Task OrderApproved_ShouldStartProcessing_AndCancelApprovalTimeout()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingApproval(orderId);

        var instance = await Advance(orderId, new OrderApprovedIntegration(orderId), m => m.AwaitingProcessing);

        (await WasSent<OrderProcess>(m => m.OrderId == orderId)).Should().BeTrue();
        instance.ApprovalTimeoutTokenId.Should().BeNull();
    }

    [Fact]
    public async Task OrderStartedProcessing_ShouldRequestPayment_WithTheStoredAmount()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingProcessing(orderId);

        await Advance(orderId, new OrderStartedProcessingIntegration(orderId), m => m.AwaitingPayment);

        (await WasSent<CreatePayment>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task OrderStartedProcessing_ShouldCarryOverCurrencyAndAmount_FromOrderPlaced()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingApproval(orderId, 99.99m, Currency.Gbp);
        await Advance(orderId, new OrderApprovedIntegration(orderId), m => m.AwaitingProcessing);

        await Advance(orderId, new OrderStartedProcessingIntegration(orderId), m => m.AwaitingPayment);

        (await WasSent<CreatePayment>(m =>
            m.OrderId == orderId && m.Amount == 99.99m && m.Currency == Currency.Gbp)).Should().BeTrue();
    }

    [Fact]
    public async Task OrderStartedProcessing_ShouldSchedulePaymentTimeout()
    {
        var orderId = Guid.NewGuid();

        var instance = await GivenAwaitingPayment(orderId);

        instance.PaymentTimeoutTokenId.Should().NotBeNull();
    }

    [Fact]
    public async Task PaymentSucceeded_ShouldConfirmOrder_AndCancelPaymentTimeout()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingPayment(orderId);

        var instance = await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);

        (await WasSent<ConfirmOrder>(m => m.OrderId == orderId)).Should().BeTrue();
        instance.PaymentTimeoutTokenId.Should().BeNull();
    }

    [Fact]
    public async Task OrderConfirmed_ShouldRequestDelivery()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingConfirmation(orderId);

        await Advance(orderId, new OrderConfirmedIntegration(orderId), m => m.AwaitingDelivery);

        (await WasSent<CreateDelivery>(m => m.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task DeliveryPlaced_ShouldFinalizeTheSaga()
    {
        var orderId = Guid.NewGuid();
        await GivenAwaitingDelivery(orderId);

        await Harness.Bus.Publish(new DeliveryPlacedIntegration(orderId));
        (await SagaHarness.Consumed.Any<DeliveryPlacedIntegration>(x => x.Context.Message.OrderId == orderId))
            .Should().BeTrue();

        (await SagaHarness.NotExists(orderId, Harness.TestTimeout)).Should().BeNull();
    }

    [Fact]
    public async Task FullHappyPath_ShouldNeverStampFailedAt()
    {
        var orderId = Guid.NewGuid();

        var instance = await GivenAwaitingDelivery(orderId);

        instance.FailedAt.Should().BeNull();
    }

    [Fact]
    public async Task ZeroAmountOrder_ShouldStillReachPayment()
    {
        var orderId = Guid.NewGuid();

        await GivenAwaitingPayment(orderId, 0m);

        (await WasSent<CreatePayment>(m => m.OrderId == orderId && m.Amount == 0m)).Should().BeTrue();
    }

    [Fact]
    public async Task ConcurrentOrders_ShouldTrackIndependentInstances()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        await GivenAwaitingPayment(first, 10m, Currency.Usd);
        await GivenAwaitingApproval(second, 20m, Currency.Eur);

        StateOf(first).Should().Be(Machine.AwaitingPayment.Name);
        StateOf(second).Should().Be(Machine.AwaitingApproval.Name);
        Instance(first).Amount.Should().Be(10m);
        Instance(second).Amount.Should().Be(20m);
    }
}
