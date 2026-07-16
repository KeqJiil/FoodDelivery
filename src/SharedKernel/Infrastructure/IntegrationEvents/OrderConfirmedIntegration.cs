namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record OrderConfirmedIntegration(Guid Id) : IntegrationEvent;