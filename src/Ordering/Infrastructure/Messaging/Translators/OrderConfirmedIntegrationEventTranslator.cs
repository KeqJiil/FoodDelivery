using Ordering.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Ordering.Infrastructure.Messaging.Translators;

public class OrderConfirmedIntegrationEventTranslator : IIntegrationEventTranslator<OrderConfirmed>
{
    public IntegrationEvent? Translate(OrderConfirmed domainEvent)
    {
        throw new NotImplementedException();
    }
}