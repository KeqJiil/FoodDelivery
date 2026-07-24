using Deliveries.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Deliveries.Infrastructure.Messaging.Translators;

public class DeliveryFailedIntegrationEventTranslator : IIntegrationEventTranslator<DeliveryFailed>
{
    public IntegrationEvent? Translate(DeliveryFailed domainEvent)
    {
        return new DeliveryFailedIntegration(domainEvent.OrderRefId.Id, domainEvent.Reason);
    }
}
