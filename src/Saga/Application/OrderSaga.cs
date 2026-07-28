using MassTransit;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace Saga.Application;

public class OrderSaga : MassTransitStateMachine<OrderState>
{
    public OrderSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderApproved, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderRejected, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderFailed, x => x.CorrelateById(m => m.Message.Id));
        Event(() => OrderStartedProcessing, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderConfirmed, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentSucceeded, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentFailed, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderCancelled, x => x.CorrelateById(m => m.Message.Id));
        Event(() => DeliveryPlaced, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderRequestCancelled, x => x.CorrelateById(m => m.Message.Id));
        Event(() => PaymentCancelled, x => x.CorrelateById(m => m.Message.OrderId));

        Schedule(() => ApprovalTimeout, x => x.ApprovalTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(20);
            s.Received = r => r.CorrelateById(x => x.Message.OrderId);
        });
        Schedule(() => PaymentTimeout, x => x.PaymentTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(10);
            s.Received = r => r.CorrelateById(x => x.Message.OrderId);
        });

        Initially(
            When(OrderPlaced).Then(x =>
                {
                    x.Saga.CorrelationId = x.Message.OrderId;
                    x.Saga.Amount = x.Message.Amount;
                    x.Saga.Currency = x.Message.Currency;
                })
                .Send(x => new CreateRequest(x.Message.OrderId, x.Message.RestaurantId))
                .Schedule(ApprovalTimeout, x => new ApprovalTimeoutExpired(x.Message.OrderId))
                .TransitionTo(AwaitingApproval));

        During(AwaitingApproval,
            When(OrderApproved)
                .Send(x => new OrderProcess(x.Message.OrderId))
                .Unschedule(ApprovalTimeout)
                .TransitionTo(AwaitingProcessing),
            When(OrderRejected)
                .Then(x => x.Saga.FailedAt = DateTime.UtcNow)
                .Unschedule(ApprovalTimeout)
                .Send(x => new OrderFail(x.Message.OrderId))
                .TransitionTo(CompensatingOrder),
            When(OrderCancelled)
                .Then(x => x.Saga.FailedAt = DateTime.UtcNow)
                .Unschedule(ApprovalTimeout)
                .Send(x => new CancelOrderRequest(x.Message.Id))
                .TransitionTo(CompensatingRequest),
            When(ApprovalTimeout.Received)
                .Then(x => x.Saga.FailedAt = DateTime.UtcNow)
                .Unschedule(ApprovalTimeout)
                .Send(x => new CancelOrderRequest(x.Message.OrderId))
                .TransitionTo(CompensatingRequest),
            Ignore(OrderPlaced));

        During(AwaitingProcessing,
            When(OrderStartedProcessing)
                .Send(x => new CreatePayment(x.Message.OrderId, x.Saga.Amount, x.Saga.Currency))
                .Schedule(PaymentTimeout, x => new PaymentTimeoutExpired(x.Message.OrderId))
                .TransitionTo(AwaitingPayment),
            Ignore(OrderCancelled),
            Ignore(ApprovalTimeout.Received),
            Ignore(OrderApproved),
            Ignore(OrderRejected));

        During(AwaitingPayment,
            When(PaymentSucceeded)
                .Send(x => new ConfirmOrder(x.Message.OrderId))
                .Unschedule(PaymentTimeout)
                .TransitionTo(AwaitingConfirmation),
            When(PaymentFailed)
                .Then(ctx => ctx.Saga.FailedAt = DateTime.UtcNow)
                .Unschedule(PaymentTimeout)
                .Send(x => new CancelOrderRequest(x.Message.OrderId))
                .TransitionTo(CompensatingRequest),
            When(PaymentTimeout.Received)
                .Then(ctx => ctx.Saga.FailedAt = DateTime.UtcNow)
                .Unschedule(PaymentTimeout)
                .Send(x => new CancelPayment(x.Message.OrderId))
                .TransitionTo(CompensatingPayment),
            Ignore(OrderCancelled),
            Ignore(ApprovalTimeout.Received),
            Ignore(OrderStartedProcessing));

        During(AwaitingConfirmation,
            When(OrderConfirmed)
                .Send(x => new CreateDelivery(x.Message.OrderId))
                .TransitionTo(AwaitingDelivery),
            Ignore(OrderCancelled),
            Ignore(ApprovalTimeout.Received),
            Ignore(PaymentTimeout.Received),
            Ignore(PaymentSucceeded),
            Ignore(PaymentFailed));


        During(AwaitingDelivery,
            When(DeliveryPlaced)
                .TransitionTo(Completed)
                .Finalize(),
            Ignore(OrderCancelled),
            Ignore(ApprovalTimeout.Received),
            Ignore(PaymentTimeout.Received),
            Ignore(OrderConfirmed));

        // Compensating

        During(CompensatingPayment,
            When(PaymentCancelled)
                .Send(x => new CancelOrderRequest(x.Message.OrderId))
                .TransitionTo(CompensatingRequest),
            When(PaymentSucceeded)
                .Send(x => new ConfirmOrder(x.Message.OrderId))
                .Then(x => x.Saga.FailedAt = null)
                .TransitionTo(AwaitingConfirmation),
            Ignore(OrderApproved),
            Ignore(OrderRejected),
            Ignore(ApprovalTimeout.Received),
            Ignore(PaymentFailed));

        During(CompensatingRequest,
            When(OrderRequestCancelled)
                .Send(x => new OrderFail(x.Message.Id))
                .TransitionTo(CompensatingOrder),
            Ignore(ApprovalTimeout.Received),
            Ignore(PaymentTimeout.Received));

        During(CompensatingOrder,
            When(OrderFailed)
                .TransitionTo(Failed)
                .Finalize(),
            Ignore(ApprovalTimeout.Received),
            Ignore(PaymentTimeout.Received),
            Ignore(OrderCancelled),
            Ignore(OrderRequestCancelled));

        SetCompletedWhenFinalized();

        // Every event this saga listens to can legitimately arrive while the instance is in a
        // state that has no business reaction to it — a duplicate from at-least-once delivery, a
        // stale message that lost a race, or a message for a step the instance has already moved
        // past. None of that should crash the consumer and drive the message to the error queue;
        // it should be dropped. See OrderSagaFullMatrixTests for the exhaustive state x event proof.
        OnUnhandledEvent(x => x.Ignore());
    }

    public State AwaitingApproval { get; private set; }
    public State AwaitingProcessing { get; private set; }
    public State AwaitingPayment { get; private set; }
    public State AwaitingConfirmation { get; private set; }
    public State AwaitingDelivery { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    public State CompensatingOrder { get; private set; }
    public State CompensatingRequest { get; private set; }
    public State CompensatingPayment { get; private set; }

    public Event<OrderPlacedIntegration> OrderPlaced { get; private set; }
    public Event<OrderApprovedIntegration> OrderApproved { get; private set; }
    public Event<OrderRejectedIntegration> OrderRejected { get; private set; }
    public Event<OrderFailedIntegration> OrderFailed { get; private set; }
    public Event<OrderStartedProcessingIntegration> OrderStartedProcessing { get; private set; }
    public Event<OrderConfirmedIntegration> OrderConfirmed { get; private set; }
    public Event<OrderCancelledIntegration> OrderCancelled { get; private set; }
    public Event<OrderRequestCancelledIntegration> OrderRequestCancelled { get; private set; }
    public Event<PaymentSucceededIntegration> PaymentSucceeded { get; private set; }
    public Event<PaymentFailedIntegration> PaymentFailed { get; private set; }
    public Event<PaymentCancelledIntegration> PaymentCancelled { get; private set; }
    public Event<DeliveryPlacedIntegration> DeliveryPlaced { get; private set; }

    public Schedule<OrderState, ApprovalTimeoutExpired> ApprovalTimeout { get; private set; }
    public Schedule<OrderState, PaymentTimeoutExpired> PaymentTimeout { get; private set; }
}

public record ApprovalTimeoutExpired(Guid OrderId);

public record PaymentTimeoutExpired(Guid OrderId);