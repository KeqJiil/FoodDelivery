using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.Incoming;

public record CreatePayment(Guid OrderId, decimal Amount, Currency Currency) : IntegrationEvent;