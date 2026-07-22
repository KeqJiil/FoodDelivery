using SharedKernel.Domain.Enums;

namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record PaymentSucceededIntegration(Guid OrderId, decimal Amount, Currency Currency) : IntegrationEvent;
