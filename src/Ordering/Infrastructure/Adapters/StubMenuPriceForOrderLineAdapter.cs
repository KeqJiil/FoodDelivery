using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Infrastructure.Adapters;

public sealed class StubMenuPriceForOrderLineAdapter : IMenuPriceForOrderLineAdapter
{
    public Task<Money?> GetMenuItemPriceByIdAsync(MenuItemRefId id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Money?>(new Money(Currency.Usd, 9.99m));
    }
}
