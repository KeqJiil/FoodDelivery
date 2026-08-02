using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Saga.Application;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace FoodDelivery.IntegrationTest.Saga;

public class SagaTimeoutTests(MsSqlContainerFixture fixture) : OrderSagaIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ApprovalTimeoutToken_ShouldBePersisted_WhileAwaitingApproval()
    {
        var orderId = Guid.CreateVersion7();

        await GivenAwaitingApproval(orderId);

        var persisted = await LoadFromDbOrThrowAsync(orderId);
        persisted.ApprovalTimeoutTokenId.Should().NotBeNull();
    }

    [Fact]
    public async Task ApprovalTimeout_ShouldFire_AndMoveSagaToCompensatingRequest()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingApproval(orderId);
        var instance = await LoadFromDbOrThrowAsync(orderId);

        await FireTimeout(new ApprovalTimeoutExpired(orderId), instance.ApprovalTimeoutTokenId);

        var instanceId = await SagaHarness.Exists(orderId, m => m.CompensatingRequest, Harness.TestTimeout);
        instanceId.Should().NotBeNull();
        (await Harness.Sent.Any<CancelOrderRequest>(m => m.Context.Message.OrderId == orderId)).Should().BeTrue();

        var afterFiring = await LoadFromDbOrThrowAsync(orderId);
        afterFiring.FailedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ApprovalTimeoutToken_ShouldBeCleared_WhenOrderIsApprovedBeforeTheTimeout()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingApproval(orderId);

        await Advance(orderId, new OrderApprovedIntegration(orderId), m => m.AwaitingProcessing);

        var persisted = await LoadFromDbOrThrowAsync(orderId);
        persisted.ApprovalTimeoutTokenId.Should().BeNull();
    }

    [Fact]
    public async Task PaymentTimeout_ShouldFire_AndMoveSagaToCompensatingPayment_ViaCancelPayment()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingPayment(orderId);
        var instance = await LoadFromDbOrThrowAsync(orderId);
        instance.PaymentTimeoutTokenId.Should().NotBeNull();

        await FireTimeout(new PaymentTimeoutExpired(orderId), instance.PaymentTimeoutTokenId);

        var instanceId = await SagaHarness.Exists(orderId, m => m.CompensatingPayment, Harness.TestTimeout);
        instanceId.Should().NotBeNull();
        (await Harness.Sent.Any<CancelPayment>(m => m.Context.Message.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task PaymentTimeoutToken_ShouldBeCleared_WhenPaymentSucceedsBeforeTheTimeout()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingPayment(orderId);

        await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);

        var persisted = await LoadFromDbOrThrowAsync(orderId);
        persisted.PaymentTimeoutTokenId.Should().BeNull();
    }

    [Fact]
    public async Task LatePaymentSuccess_AfterTimeoutAlreadyFired_ShouldRecoverSagaToAwaitingConfirmation()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingPayment(orderId);
        var instance = await LoadFromDbOrThrowAsync(orderId);

        await FireTimeout(new PaymentTimeoutExpired(orderId), instance.PaymentTimeoutTokenId);
        await SagaHarness.Exists(orderId, m => m.CompensatingPayment, Harness.TestTimeout);
        var compensating = await LoadFromDbOrThrowAsync(orderId);
        compensating.FailedAt.Should().NotBeNull();

        await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);

        (await Harness.Sent.Any<ConfirmOrder>(m => m.Context.Message.OrderId == orderId)).Should().BeTrue();

        var recovered = await LoadFromDbOrThrowAsync(orderId);
        recovered.FailedAt.Should().BeNull();
    }
}
