using OrderRequests.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace OrderRequests.Infrastructure.Messaging.Translators;

public class OrderRequestCancelledIntegrationTranslator : IIntegrationEventTranslator<OrderCancelled>
{
    public IntegrationEvent Translate(OrderCancelled message)
    {
        return new OrderRequestCancelledIntegration(message.Id.Id);
    }
}