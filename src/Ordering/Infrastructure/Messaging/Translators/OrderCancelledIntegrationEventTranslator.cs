using Ordering.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Ordering.Infrastructure.Messaging.Translators;

public class OrderCancelledIntegrationEventTranslator : IIntegrationEventTranslator<OrderCancelled>
{
    public IntegrationEvent? Translate(OrderCancelled domainEvent)
    {
        throw new NotImplementedException();
    }
}