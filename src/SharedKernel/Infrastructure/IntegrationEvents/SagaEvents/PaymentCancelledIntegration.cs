using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public record PaymentCancelledIntegration(Guid OrderId) : IntegrationEvent;