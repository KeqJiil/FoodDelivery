using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public record OrderApprovedIntegration(Guid OrderId) : IntegrationEvent;