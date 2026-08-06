using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using Restaurants.Application.Abstractions;

namespace Ordering.Infrastructure.Adapters;

public sealed class RestaurantActiveAdapter(IRestaurantReader restaurantReader) : IRestaurantActiveAdapter
{
    public async Task<bool> IsActiveAsync(RestaurantRefId id, CancellationToken cancellationToken = default)
    {
        return await restaurantReader.IsActiveAsync(id.Id, cancellationToken) ?? false;
    }
}