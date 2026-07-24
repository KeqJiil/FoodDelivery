using SharedKernel.Domain.Enums;

namespace SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

public sealed record MenuItemPriceChangedIntegration(Guid MenuId, decimal Amount, Currency Currency)
    : IntegrationEvent;