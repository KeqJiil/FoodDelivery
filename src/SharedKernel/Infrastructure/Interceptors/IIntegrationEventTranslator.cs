using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.Interceptors;

public interface IIntegrationEventTranslator<in TDomainEvent>
{
    IntegrationEvent? Translate(TDomainEvent domainEvent);
}