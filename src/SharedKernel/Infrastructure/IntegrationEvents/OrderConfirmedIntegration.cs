using SharedKernel.Domain.Enums;

namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record OrderConfirmedIntegration(Guid Id, decimal Amount, Currency Currency) : IntegrationEvent;
