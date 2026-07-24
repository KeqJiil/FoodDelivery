using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public sealed record OrderConfirmedIntegration(Guid Id, decimal Amount, Currency Currency) : IntegrationEvent;