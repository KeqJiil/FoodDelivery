using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Saga.Application;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Messaging;

namespace Saga.UnitTest.TestHelpers;

public abstract class OrderSagaTestBase : IAsyncLifetime
{
    static OrderSagaTestBase()
    {
        Queues.RegisterEndpointConventions();
    }

    private ServiceProvider _provider = null!;

    protected ITestHarness Harness { get; private set; } = null!;
    protected ISagaStateMachineTestHarness<OrderSaga, OrderState> SagaHarness { get; private set; } = null!;
    protected OrderSaga Machine { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddDelayedMessageScheduler();
                x.AddSagaStateMachine<OrderSaga, OrderState>().InMemoryRepository();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseDelayedMessageScheduler();
                    cfg.ConfigureEndpoints(context);
                });
            })
            .BuildServiceProvider(true);

        Harness = _provider.GetRequiredService<ITestHarness>();
        Harness.TestTimeout = TimeSpan.FromSeconds(10);
        await Harness.Start();

        SagaHarness = Harness.GetSagaStateMachineHarness<OrderSaga, OrderState>();
        Machine = _provider.GetRequiredService<OrderSaga>();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    protected static OrderPlacedIntegration Placed(Guid orderId, decimal amount = 50m,
        Currency currency = Currency.Usd) => new(orderId, Guid.NewGuid(), amount, currency);

    protected async Task<OrderState> Advance<TEvent>(Guid orderId, TEvent message, Func<OrderSaga, State> state)
        where TEvent : class
    {
        await Harness.Bus.Publish(message);
        var instanceId = await SagaHarness.Exists(orderId, state, Harness.TestTimeout);
        Assert.NotNull(instanceId);
        return Instance(orderId);
    }
    
    protected Task FireTimeout<TMessage>(TMessage message, Guid? tokenId) where TMessage : class =>
        Harness.Bus.Publish(message, ctx =>
        {
            if (tokenId.HasValue)
                ctx.Headers.Set(MessageHeaders.SchedulingTokenId, tokenId.Value.ToString());
        });

    protected OrderState Instance(Guid orderId)
    {
        var instance = SagaHarness.Sagas.Contains(orderId);
        Assert.NotNull(instance);
        return instance;
    }

    protected string StateOf(Guid orderId) => Instance(orderId).CurrentState;
    
    protected Task<bool> WasSent<TCommand>(Func<TCommand, bool> matches) where TCommand : class =>
        Harness.Sent.Any<TCommand>(x => matches(x.Context.Message));
    
    protected async Task<bool> NothingSent<TCommand>(Func<TCommand, bool> matches) where TCommand : class
    {
        await Harness.InactivityTask;
        return !Harness.Sent.Select<TCommand>(x => matches(x.Context.Message)).Any();
    }

    protected Task<OrderState> GivenAwaitingApproval(Guid orderId, decimal amount = 50m,
        Currency currency = Currency.Usd) =>
        Advance(orderId, Placed(orderId, amount, currency), m => m.AwaitingApproval);

    protected async Task<OrderState> GivenAwaitingProcessing(Guid orderId)
    {
        await GivenAwaitingApproval(orderId);
        return await Advance(orderId, new OrderApprovedIntegration(orderId), m => m.AwaitingProcessing);
    }

    protected async Task<OrderState> GivenAwaitingPayment(Guid orderId, decimal amount = 50m,
        Currency currency = Currency.Usd)
    {
        await GivenAwaitingApproval(orderId, amount, currency);
        await Advance(orderId, new OrderApprovedIntegration(orderId), m => m.AwaitingProcessing);
        return await Advance(orderId, new OrderStartedProcessingIntegration(orderId), m => m.AwaitingPayment);
    }

    protected async Task<OrderState> GivenAwaitingConfirmation(Guid orderId)
    {
        await GivenAwaitingPayment(orderId);
        return await Advance(orderId, new PaymentSucceededIntegration(orderId), m => m.AwaitingConfirmation);
    }

    protected async Task<OrderState> GivenAwaitingDelivery(Guid orderId)
    {
        await GivenAwaitingConfirmation(orderId);
        return await Advance(orderId, new OrderConfirmedIntegration(orderId), m => m.AwaitingDelivery);
    }

    protected async Task<OrderState> GivenCompensatingRequest(Guid orderId)
    {
        await GivenAwaitingApproval(orderId);
        return await Advance(orderId, new OrderCancelledIntegration(orderId), m => m.CompensatingRequest);
    }

    protected async Task<OrderState> GivenCompensatingPayment(Guid orderId)
    {
        var instance = await GivenAwaitingPayment(orderId);
        await FireTimeout(new PaymentTimeoutExpired(orderId), instance.PaymentTimeoutTokenId);

        var instanceId = await SagaHarness.Exists(orderId, m => m.CompensatingPayment, Harness.TestTimeout);
        Assert.NotNull(instanceId);
        return Instance(orderId);
    }

    protected async Task<OrderState> GivenCompensatingOrder(Guid orderId)
    {
        await GivenAwaitingApproval(orderId);
        return await Advance(orderId, new OrderRejectedIntegration(orderId), m => m.CompensatingOrder);
    }
}
