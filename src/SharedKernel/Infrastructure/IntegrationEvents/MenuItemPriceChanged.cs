using SharedKernel.Domain.Enums;

namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record MenuItemPriceChangedIntegration(Guid Id, decimal Amount, Currency Currency) : IntegrationEvent;