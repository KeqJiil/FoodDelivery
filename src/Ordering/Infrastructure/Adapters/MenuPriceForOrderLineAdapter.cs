using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Infrastructure.Adapters;

public sealed class MenuPriceForOrderLineAdapter(IRestaurantReader restaurantReader) : IMenuPriceForOrderLineAdapter
{
    public Task<Money?> GetMenuItemPriceByIdAsync(MenuItemRefId id, CancellationToken cancellationToken = default)
    {
        return restaurantReader.GetMenuItemPriceByIdAsync(id.Id, cancellationToken);
    }
}
