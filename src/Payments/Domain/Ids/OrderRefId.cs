using SharedKernel.Domain;

namespace Payments.Domain.Ids;

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
