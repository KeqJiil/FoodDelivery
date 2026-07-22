using OrderRequests.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;
using SharedKernel.Infrastructure.Interceptors;

namespace OrderRequests.Infrastructure.Messaging.Translators;

public class OrderApprovedIntegrationEventTranslator : IIntegrationEventTranslator<OrderApproved>
{
    public IntegrationEvent? Translate(OrderApproved domainEvent)
    {
        return new ConfirmOrder(domainEvent.OrderRefId.Id);
    }
}
