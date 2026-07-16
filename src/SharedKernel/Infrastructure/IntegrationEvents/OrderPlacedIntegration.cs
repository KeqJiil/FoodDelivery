namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record OrderPlacedIntegration(Guid Id) : IntegrationEvent;