using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public record OrderStartedProcessingIntegration(Guid Id) : IntegrationEvent;