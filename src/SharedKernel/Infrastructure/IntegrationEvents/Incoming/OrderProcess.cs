using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.Incoming;

public record OrderProcess(Guid OrderId) : IntegrationEvent;