using Deliveries.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Deliveries.Infrastructure.Messaging.Translators;

public class DeliveryPlacedTranslator : IIntegrationEventTranslator<DeliveryCreated>
{
    public IntegrationEvent? Translate(DeliveryCreated domainEvent)
    {
        return new DeliveryPlacedIntegration(domainEvent.OrderRefId.Id);
    }
}