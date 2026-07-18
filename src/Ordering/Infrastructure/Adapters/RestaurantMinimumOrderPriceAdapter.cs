using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Infrastructure.Adapters;

public sealed class RestaurantMinimumOrderPriceAdapter(IRestaurantReader restaurantReader)
    : IRestaurantMinimumOrderPriceAdapter
{
    public async Task<Money?> GetMinimumPriceForOrderAsync(RestaurantRefId id, CancellationToken cancellationToken = default)
    {
        var restaurant = await restaurantReader.GetByIdAsync(id.Id, cancellationToken);

        return restaurant is not null ? restaurant.MinimalOrderPrice : null;
    }
}
