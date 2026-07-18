using SharedKernel.Domain;

namespace OrderRequests.Domain.Ids;

public record RestaurantRefId : TypedId
{
    public RestaurantRefId()
    {
    }

    public RestaurantRefId(Guid id)
        : base(id)
    {
    }
};