using SharedKernel.Domain;

namespace Restaurants.Domain.Ids;

public record OrderRefId : TypedId
{
    public OrderRefId(Guid id) : base(id)
    {
    }
}