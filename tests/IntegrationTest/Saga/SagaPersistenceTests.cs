using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Saga.Application;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace FoodDelivery.IntegrationTest.Saga;

public class SagaPersistenceTests(MsSqlContainerFixture fixture) : OrderSagaIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Place_ShouldPersistOrderStateInSagaDbContext_NotJustInMemory()
    {
        var orderId = Guid.CreateVersion7();

        await GivenAwaitingApproval(orderId);

        var persisted = await LoadFromDbOrThrowAsync(orderId);
        persisted.CurrentState.Should().Be(nameof(OrderSaga.AwaitingApproval));
        persisted.CorrelationId.Should().Be(orderId);
    }

    [Fact]
    public async Task State_ShouldBeLoadedFromDatabase_NotFromTheOriginalProcessMemory()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingApproval(orderId);

        await using var provider2 = BuildProvider();
        var harness2 = provider2.GetRequiredService<MassTransit.Testing.ITestHarness>();
        harness2.TestTimeout = TimeSpan.FromSeconds(10);
        await harness2.Start();
        var sagaHarness2 = harness2.GetSagaStateMachineHarness<OrderSaga, OrderState>();

        await harness2.Bus.Publish(new OrderApprovedIntegration(orderId));
        var instanceId = await sagaHarness2.Exists(orderId, m => m.AwaitingProcessing, harness2.TestTimeout);

        instanceId.Should().NotBeNull();
    }

    [Fact]
    public async Task AmountCurrencyAndTimeoutToken_ShouldPersistAcrossSaveChanges()
    {
        var orderId = Guid.CreateVersion7();

        await GivenAwaitingApproval(orderId, amount: 123.45m, currency: SharedKernel.Domain.Enums.Currency.Eur);

        var persisted = await LoadFromDbOrThrowAsync(orderId);
        persisted.Amount.Should().Be(123.45m);
        persisted.Currency.Should().Be(SharedKernel.Domain.Enums.Currency.Eur);
        persisted.ApprovalTimeoutTokenId.Should().NotBeNull();
        persisted.FailedAt.Should().BeNull();
    }

    [Fact]
    public async Task FailedAt_ShouldPersist_WhenSagaEntersACompensatingState()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingApproval(orderId);

        await Advance(orderId, new OrderRejectedIntegration(orderId), m => m.CompensatingOrder);

        var persisted = await LoadFromDbOrThrowAsync(orderId);
        persisted.FailedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Row_ShouldBeDeleted_WhenSagaCompletesSuccessfully()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingDelivery(orderId);

        await Harness.Bus.Publish(new DeliveryPlacedIntegration(orderId));
        await Eventually.AssertAsync(async () => (await LoadFromDbAsync(orderId)).Should().BeNull());
    }

    [Fact]
    public async Task Row_ShouldBeDeleted_WhenSagaEndsInFailed()
    {
        var orderId = Guid.CreateVersion7();
        await GivenAwaitingApproval(orderId);
        await Advance(orderId, new OrderRejectedIntegration(orderId), m => m.CompensatingOrder);

        await Harness.Bus.Publish(new OrderFailedIntegration(orderId));
        await Eventually.AssertAsync(async () => (await LoadFromDbAsync(orderId)).Should().BeNull());
    }
}
