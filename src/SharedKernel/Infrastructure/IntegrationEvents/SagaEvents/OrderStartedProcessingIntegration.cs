using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public record OrderStartedProcessingIntegration(Guid OrderId, decimal Amount, Currency Currency) : IntegrationEvent;