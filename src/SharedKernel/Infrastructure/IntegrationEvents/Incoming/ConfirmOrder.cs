using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.Incoming;

public sealed record ConfirmOrder(Guid OrderId) : IntegrationEvent;