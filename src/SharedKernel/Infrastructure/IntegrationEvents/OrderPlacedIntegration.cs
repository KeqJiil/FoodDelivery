namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record OrderPlacedIntegration(Guid OrderId, Guid RestaurantId) : IntegrationEvent;