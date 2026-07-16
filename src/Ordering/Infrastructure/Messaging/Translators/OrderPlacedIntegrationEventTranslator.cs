using Ordering.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Ordering.Infrastructure.Messaging.Translators;

public class OrderPlacedIntegrationEventTranslator : IIntegrationEventTranslator<OrderPlaced>
{
    public IntegrationEvent? Translate(OrderPlaced domainEvent)
    {
        throw new NotImplementedException();
    }
}