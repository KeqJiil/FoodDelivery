using MassTransit;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace Saga.Domain;

public class OrderSaga : MassTransitStateMachine<OrderState>
{
    public OrderSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderApproved, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderConfirmed, x => x.CorrelateById(m => m.Message.Id));
        Event(() => PaymentSucceeded, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => DeliveryPlaced, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(OrderPlaced).Then<OrderState, OrderPlacedIntegration>(x => x.Saga.CorrelationId = x.Message.OrderId)
                .Send(x => new CreateRequest(x.Message.OrderId, x.Message.RestaurantId))
                .TransitionTo<OrderState, OrderPlacedIntegration>(AwaitingApproval));
        
    }
    
    public State AwaitingApproval { get; private set; }
    public State AwaitingPayment { get; private set; }
    public State AwaitingDelivery { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    public Event<OrderPlacedIntegration> OrderPlaced { get; private set; }
    public Event<OrderApprovedIntegration> OrderApproved { get; private set; }
    public Event<OrderConfirmedIntegration> OrderConfirmed { get; private set; }
    public Event<PaymentSucceededIntegration> PaymentSucceeded { get; private set; }
    public Event<DeliveryPlacedIntegration> DeliveryPlaced { get; private set; }
}