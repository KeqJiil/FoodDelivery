using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public sealed record PaymentFailedIntegration(Guid OrderId, string Reason) : IntegrationEvent;