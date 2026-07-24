using Deliveries.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Deliveries.Infrastructure.Messaging.Translators;

public class DeliveryCompletedIntegrationEventTranslator : IIntegrationEventTranslator<DeliveryCompleted>
{
    public IntegrationEvent? Translate(DeliveryCompleted domainEvent)
    {
        return new DeliveryCompletedIntegration(domainEvent.OrderRefId.Id);
    }
}
