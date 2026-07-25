using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.Incoming;

public sealed record CancelOrderRequest(Guid OrderId) : IntegrationEvent;