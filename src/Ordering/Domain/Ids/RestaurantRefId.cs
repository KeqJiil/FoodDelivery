using SharedKernel.Domain;

namespace Ordering.Domain.Ids;

public record RestaurantRefId : TypedId
{
    public RestaurantRefId()
    {
    }

    public RestaurantRefId(Guid id)
        : base(id)
    {
    }
}