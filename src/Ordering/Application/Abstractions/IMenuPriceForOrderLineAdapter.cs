using Ordering.Domain.Ids;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Application.Abstractions;

public interface IMenuPriceForOrderLineAdapter
{
    public Task<Money?> GetMenuItemPriceByIdAsync(MenuItemRefId id, CancellationToken cancellationToken = default);
}