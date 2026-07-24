using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public record OrderRejectedIntegration(Guid OrderId) : IntegrationEvent;