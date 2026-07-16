namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record OrderCancelledIntegration(Guid Id) : IntegrationEvent;