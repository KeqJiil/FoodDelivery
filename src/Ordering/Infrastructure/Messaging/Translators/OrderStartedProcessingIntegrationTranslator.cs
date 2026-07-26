using Ordering.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Ordering.Infrastructure.Messaging.Translators;

public class OrderStartedProcessingIntegrationTranslator : IIntegrationEventTranslator<OrderStartedProcessing>
{
    public IntegrationEvent? Translate(OrderStartedProcessing domainEvent)
    {
        return new OrderStartedProcessingIntegration(domainEvent.Id.Id, domainEvent.Price.Amount,
            domainEvent.Price.Currency);
    }
}