using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.Incoming;

public record OrderFail(Guid OrderId) : IntegrationEvent;