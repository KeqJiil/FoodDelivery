using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Ids;

namespace Ordering.Application.Abstractions;

public interface IMenuPriceForOrderLineAdapter
{
    public Task<Money?> GetMenuItemPriceByIdAsync(MenuItemRefId id, CancellationToken cancellationToken = default);
}