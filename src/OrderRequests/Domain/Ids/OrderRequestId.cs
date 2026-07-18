using SharedKernel.Domain;

namespace OrderRequests.Domain.Ids;

public record OrderRequestId : TypedId
{
    public OrderRequestId()
    {
    }

    public OrderRequestId(Guid id)
        : base(id)
    {
    }
};