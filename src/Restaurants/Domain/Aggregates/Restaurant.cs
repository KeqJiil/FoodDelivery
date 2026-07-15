using Restaurants.Domain.Ids;
using SharedKernel.Domain;

namespace Restaurants.Domain.Aggregates;

public class Restaurant : AggregateRoot<RestaurantId>
{
    public RestaurantId Id { get; }

    // TODO
}