using Ordering.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Ordering.Infrastructure.Messaging.Translators;

public class OrderPlacedIntegrationEventTranslator : IIntegrationEventTranslator<OrderPlaced>
{
    public IntegrationEvent? Translate(OrderPlaced domainEvent)
    {
        return new OrderPlacedIntegration(domainEvent.AggregateId.Id, domainEvent.RestaurantRefId.Id,
            domainEvent.Price.Amount, domainEvent.Price.Currency);
    }
}