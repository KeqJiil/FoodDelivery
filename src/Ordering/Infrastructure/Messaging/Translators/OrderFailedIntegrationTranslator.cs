using Ordering.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Ordering.Infrastructure.Messaging.Translators;

public class OrderFailedIntegrationTranslator : IIntegrationEventTranslator<OrderFailed>
{
    public IntegrationEvent? Translate(OrderFailed domainEvent)
    {
        return new OrderFailedIntegration(domainEvent.Id.Id);
    }
}