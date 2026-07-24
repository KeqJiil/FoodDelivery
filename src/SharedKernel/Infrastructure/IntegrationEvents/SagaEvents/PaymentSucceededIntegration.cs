using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public sealed record PaymentSucceededIntegration(Guid OrderId) : IntegrationEvent;