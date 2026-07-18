using Restaurants.Application.GetRestaurantById;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.Abstractions;

public interface IRestaurantReader
{
    Task<RestaurantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Money?> GetMenuItemPriceByIdAsync(Guid menuItemId, CancellationToken cancellationToken = default);
}
