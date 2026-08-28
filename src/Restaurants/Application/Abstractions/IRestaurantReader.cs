using Restaurants.Application.GetRestaurantById;
using Restaurants.Application.GetRestaurantsList;
using Restaurants.Domain.Aggregates;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.Abstractions;

public interface IRestaurantReader
{
    Task<RestaurantsListPagination> GetRestaurants(int page, int pageSize, CancellationToken ct = default);
    Task<RestaurantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Money?> GetMenuItemPriceByIdAsync(Guid menuItemId, CancellationToken cancellationToken = default);
    Task<bool?> IsActiveAsync(Guid id, CancellationToken cancellationToken = default);
}