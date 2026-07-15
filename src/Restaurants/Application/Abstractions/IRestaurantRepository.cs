using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Ids;

namespace Restaurants.Application.Abstractions;

public interface IRestaurantRepository
{
    public Task<Restaurant?> GetById(RestaurantId id, CancellationToken cancellationToken = default);
    public void Add(Restaurant restaurant);
}