namespace SharedKernel.Infrastructure.IntegrationEvents.Incoming;

public sealed record CancelOrder(Guid OrderId) : IntegrationEvent;
