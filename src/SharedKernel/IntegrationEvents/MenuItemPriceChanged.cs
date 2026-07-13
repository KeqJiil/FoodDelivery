using SharedKernel.Domain.Enums;

namespace SharedKernel.IntegrationEvents;

public sealed record MenuItemPriceChanged(Guid MenuItemId, decimal Amount, Currency Currency);