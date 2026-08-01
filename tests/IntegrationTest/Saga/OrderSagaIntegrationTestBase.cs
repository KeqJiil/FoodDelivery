using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Saga.Application;
using Saga.Infrastructure.Persistence;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Options;

namespace FoodDelivery.IntegrationTest.Saga;

[Collection("Database")]
public abstract class OrderSagaIntegrationTestBase(MsSqlContainerFixture fixture) : IAsyncLifetime
{
    static OrderSagaIntegrationTestBase() => SharedKernel.Infrastructure.Messaging.Queues.RegisterEndpointConventions();

    private ServiceProvider _provider = null!;

    protected ITestHarness Harness { get; private set; } = null!;
    protected ISagaStateMachineTestHarness<OrderSaga, OrderState> SagaHarness { get; private set; } = null!;

    protected virtual SagaOptions Options => new() { TimeoutApprovement = 1, TimeoutPayment = 1 };

    public async Task InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();
        _provider = BuildProvider();
        Harness = _provider.GetRequiredService<ITestHarness>();
        Harness.TestTimeout = TimeSpan.FromSeconds(10);
        await Harness.Start();
        SagaHarness = Harness.GetSagaStateMachineHarness<OrderSaga, OrderState>();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    protected ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(Options));
        services.AddDbContext<SagaDbContext>(o => o.UseSqlServer(fixture.ConnectionString,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "saga")));
        services.AddMassTransitTestHarness(x =>
        {
            x.AddDelayedMessageScheduler();
            x.AddSagaStateMachine<OrderSaga, OrderState>().EntityFrameworkRepository(r =>
            {
                r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                r.ExistingDbContext<SagaDbContext>();
            });
            x.UsingInMemory((context, cfg) =>
            {
                cfg.UseDelayedMessageScheduler();
                cfg.ConfigureEndpoints(context);
            });
        });

        return services.BuildServiceProvider(true);
    }

    protected static OrderPlacedIntegration Placed(Guid orderId, decimal amount = 50m,
        Currency currency = Currency.Usd) => new(orderId, Guid.CreateVersion7(), amount, currency);

    protected async Task<Guid?> Advance<TEvent>(Guid orderId, TEvent message, Func<OrderSaga, State> state)
        where TEvent : class
    {
        await Harness.Bus.Publish(message);
        var instanceId = await SagaHarness.Exists(orderId, state, Harness.TestTimeout);
        instanceId.Should().NotBeNull($"the saga for {orderId} should have reached {state}");
        return instanceId;
    }

    protected Task FireTimeout<TMessage>(TMessage message, Guid? tokenId) where TMessage : class =>
        Harness.Bus.Publish(message, ctx =>
        {
            if (tokenId.HasValue)
                ctx.Headers.Set(MessageHeaders.SchedulingTokenId, tokenId.Value.ToString());
        });

    protected async Task<OrderState?> LoadFromDbAsync(Guid orderId)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SagaDbContext>();
        return await context.OrderStates.AsNoTracking().FirstOrDefaultAsync(x => x.CorrelationId == orderId);
    }

    protected async Task<OrderState> LoadFromDbOrThrowAsync(Guid orderId)
    {
        var state = await LoadFromDbAsync(orderId);
        state.Should().NotBeNull($"expected a persisted saga row for {orderId}");
        return state!;
    }

    protected async Task<int> CountRowsAsync(Guid orderId)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SagaDbContext>();
        return await context.OrderStates.CountAsync(x => x.CorrelationId == orderId);
    }

    protected Task<Guid?> GivenAwaitingApproval(Guid orderId, decimal amount = 50m, Currency currency = Currency.Usd) =>
        Advance(orderId, Placed(orderId, amount, currency), m => m.AwaitingApproval);

    protected async Task<Guid?> GivenAwaitingProcessing(Guid orderId)
    {
        await GivenAwaitingApproval(orderId);
        return await Advance(orderId, new OrderApprovedIntegration(orderId), m => m.AwaitingProcessing);
    }

    protected async Task<Guid?> GivenAwaitingPayment(Guid orderId, decimal amount = 50m, Currency currency = Currency.Usd)
    {
        await GivenAwaitingApproval(orderId, amount, currency);
        await Advance(orderId, new OrderApprovedIntegration(orderId), m => m.AwaitingProcessing);
        return await Advance(orderId, new OrderStartedProcessingIntegration(orderId), m => m.AwaitingPayment);
    }

    protected async Task<Guid?> GivenAwaitingConfirmation(Guid orderId)
    {
        await GivenAwaitingPayment(orderId);
        return await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);
    }

    protected async Task<Guid?> GivenAwaitingDelivery(Guid orderId)
    {
        await GivenAwaitingConfirmation(orderId);
        return await Advance(orderId, new OrderConfirmedIntegration(orderId), m => m.AwaitingDelivery);
    }
}
