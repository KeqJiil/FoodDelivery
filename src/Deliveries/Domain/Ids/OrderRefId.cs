using SharedKernel.Domain;

namespace Deliveries.Domain.Ids;

public record OrderRefId : TypedId
{
    public OrderRefId()
    {
    }

    public OrderRefId(Guid id)
        : base(id)
    {
    }
};
