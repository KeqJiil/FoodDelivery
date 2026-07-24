using Ordering.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Ordering.Infrastructure.Messaging.Translators;

public class OrderCancelledIntegrationEventTranslator : IIntegrationEventTranslator<OrderCancelled>
{
    public IntegrationEvent? Translate(OrderCancelled domainEvent)
    {
        return new OrderCancelledIntegration(domainEvent.AggregateId.Id);
    }
}