using OrderRequests.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace OrderRequests.Infrastructure.Messaging.Translators;

public class OrderRejectedIntegrationEventTranslator : IIntegrationEventTranslator<OrderRejected>
{
    public IntegrationEvent? Translate(OrderRejected domainEvent)
    {
        return new OrderRejectedIntegration(domainEvent.OrderRefId.Id);
    }
}
