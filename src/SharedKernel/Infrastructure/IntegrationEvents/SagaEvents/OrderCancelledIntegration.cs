using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public sealed record OrderCancelledIntegration(Guid Id) : IntegrationEvent;