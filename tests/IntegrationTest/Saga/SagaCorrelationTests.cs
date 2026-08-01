using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Saga.Application;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace FoodDelivery.IntegrationTest.Saga;

public class SagaCorrelationTests(MsSqlContainerFixture fixture) : OrderSagaIntegrationTestBase(fixture)
{
    [Fact]
    public async Task HappyPath_ShouldCorrelateEachEventToTheSameSagaInstance_ThroughToCompletion()
    {
        var orderId = Guid.CreateVersion7();

        await GivenAwaitingDelivery(orderId);
        await Harness.Bus.Publish(new DeliveryPlacedIntegration(orderId));
        await Eventually.AssertAsync(async () => (await LoadFromDbAsync(orderId)).Should().BeNull());

        (await Harness.Sent.Any<CreateRequest>(m => m.Context.Message.OrderId == orderId)).Should().BeTrue();
        (await Harness.Sent.Any<OrderProcess>(m => m.Context.Message.OrderId == orderId)).Should().BeTrue();
        (await Harness.Sent.Any<CreatePayment>(m => m.Context.Message.OrderId == orderId)).Should().BeTrue();
        (await Harness.Sent.Any<ConfirmOrder>(m => m.Context.Message.OrderId == orderId)).Should().BeTrue();
        (await Harness.Sent.Any<CreateDelivery>(m => m.Context.Message.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task PaymentFailure_ShouldCorrelate_PaymentFailedAndOrderRequestCancelledAndOrderFailed()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingPayment(orderId);

        await Advance(orderId, new PaymentFailedIntegration(orderId, "card declined"), m => m.CompensatingRequest);
        await Advance(orderId, new OrderRequestCancelledIntegration(orderId), m => m.CompensatingOrder);
        await Harness.Bus.Publish(new OrderFailedIntegration(orderId));
        await Eventually.AssertAsync(async () => (await LoadFromDbAsync(orderId)).Should().BeNull());
    }

    [Fact]
    public async Task OrderRejected_ShouldCorrelateById_NotByOrderId()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingApproval(orderId);

        await Advance(orderId, new OrderRejectedIntegration(orderId), m => m.CompensatingOrder);
        await Harness.Bus.Publish(new OrderFailedIntegration(orderId));
        await Eventually.AssertAsync(async () => (await LoadFromDbAsync(orderId)).Should().BeNull());
    }

    [Fact]
    public async Task OrderCancelled_DuringAwaitingApproval_ShouldCorrelateByMessageId_NotOrderId()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingApproval(orderId);
        
        await Advance(orderId, new OrderCancelledIntegration(orderId), m => m.CompensatingRequest);
    }

    [Fact]
    public async Task PaymentTimeoutCompensation_ShouldCorrelate_PaymentCancelledBackToConfirmation()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingPayment(orderId);
        var instance = await LoadFromDbOrThrowAsync(orderId);

        await FireTimeout(new PaymentTimeoutExpired(orderId), instance.PaymentTimeoutTokenId);
        var instanceId = await SagaHarness.Exists(orderId, m => m.CompensatingPayment, Harness.TestTimeout);
        instanceId.Should().NotBeNull();

        await Advance(orderId, new PaymentCancelledIntegration(orderId), m => m.CompensatingRequest);
    }

    [Fact]
    public async Task EventForANonExistentSaga_ShouldBeIgnored_AndNotThrow()
    {
        var unknownOrderId = Guid.CreateVersion7();

        var act = async () => await Harness.Bus.Publish(new OrderApprovedIntegration(unknownOrderId));

        await act.Should().NotThrowAsync();
        (await Harness.Consumed.Any<OrderApprovedIntegration>(m => m.Context.Message.OrderId == unknownOrderId))
            .Should().BeTrue("the message should still be delivered to the saga endpoint");
        var instance = await LoadFromDbAsync(unknownOrderId);
        instance.Should().BeNull("OnUnhandledEvent(x => x.Ignore()) means no saga instance is created for an event with no matching correlation");
    }
}
