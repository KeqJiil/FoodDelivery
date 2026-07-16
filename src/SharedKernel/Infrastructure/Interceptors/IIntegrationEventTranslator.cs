using SharedKernel.Infrastructure.IntegrationEvents;

namespace SharedKernel.Infrastructure.Interceptors;

public interface IIntegrationEventTranslator<in TDomainEvent>
{
    IntegrationEvent? Translate(TDomainEvent domainEvent);
}
