using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;

namespace Ordering.Infrastructure.Adapters;

public sealed class StubMenuPriceForOrderLineAdapter : IMenuPriceForOrderLineAdapter
{
    public Task<Money?> GetMenuItemPriceByIdAsync(MenuItemRefId id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Money?>(Money.Create(Currency.Usd, 9.99m).Ok);
    }
}