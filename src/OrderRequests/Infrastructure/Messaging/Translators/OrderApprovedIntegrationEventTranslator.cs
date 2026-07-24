using OrderRequests.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace OrderRequests.Infrastructure.Messaging.Translators;

public class OrderApprovedIntegrationEventTranslator : IIntegrationEventTranslator<OrderApproved>
{
    public IntegrationEvent? Translate(OrderApproved domainEvent)
    {
        return new OrderApprovedIntegration(domainEvent.OrderRefId.Id);
    }
}