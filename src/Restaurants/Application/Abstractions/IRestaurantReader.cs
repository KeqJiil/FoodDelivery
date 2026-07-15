using Restaurants.Application.GetRestaurantById;

namespace Restaurants.Application.Abstractions;

public interface IRestaurantReader
{
    Task<RestaurantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
