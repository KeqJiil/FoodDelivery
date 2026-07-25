using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public record OrderFailedIntegration(Guid Id) : IntegrationEvent;