using SharedKernel.Domain;

namespace Restaurants.Domain.Ids;

public record RestaurantId : TypedId
{
    public RestaurantId()
    {
    }

    public RestaurantId(Guid id) : base(id)
    {
    }
}