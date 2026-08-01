using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace FoodDelivery.IntegrationTest.Saga;

public class SagaConcurrencyTests(MsSqlContainerFixture fixture) : OrderSagaIntegrationTestBase(fixture)
{
    [Fact]
    public async Task TwoCompetingEventsOnTheSameSaga_ShouldNotCorruptState_ExactlyOneShouldWin()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingApproval(orderId);
        
        await Task.WhenAll(
            Harness.Bus.Publish(new OrderApprovedIntegration(orderId)),
            Harness.Bus.Publish(new OrderCancelledIntegration(orderId)));

        var reachedProcessing = await SagaHarness.Exists(orderId, m => m.AwaitingProcessing, TimeSpan.FromSeconds(3));
        var reachedCompensating = await SagaHarness.Exists(orderId, m => m.CompensatingRequest, TimeSpan.FromSeconds(3));

        (reachedProcessing is not null ^ reachedCompensating is not null).Should().BeTrue();
        (await CountRowsAsync(orderId)).Should().Be(1);
    }

    [Fact]
    public async Task DuplicateOrderPlaced_WhileAwaitingApproval_ShouldBeIgnored_NoSecondCreateRequestSent()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingApproval(orderId);

        await Harness.Bus.Publish(Placed(orderId));
        await Harness.Bus.Publish(Placed(orderId));

        (await Harness.Consumed.SelectAsync<OrderPlacedIntegration>(m => m.Context.Message.OrderId == orderId)
                .CountAsync())
            .Should().Be(3);
        (await Harness.Sent.SelectAsync<CreateRequest>(m => m.Context.Message.OrderId == orderId).CountAsync())
            .Should().Be(1);
    }

    [Fact]
    public async Task PlacingTheSameOrderTwice_ShouldResultInExactlyOneSagaRow()
    {
        var orderId = Guid.CreateVersion7();

        await GivenAwaitingApproval(orderId);
        await Harness.Bus.Publish(Placed(orderId));
        await Task.Delay(500);

        (await CountRowsAsync(orderId)).Should().Be(1);
    }
}
