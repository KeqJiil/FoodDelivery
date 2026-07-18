using SharedKernel.Domain;

namespace OrderRequests.Domain.Ids;

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