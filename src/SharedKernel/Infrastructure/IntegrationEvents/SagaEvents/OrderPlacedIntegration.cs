using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public sealed record OrderPlacedIntegration(Guid OrderId, Guid RestaurantId, decimal Amount, Currency Currency) : IntegrationEvent;