using SharedKernel.Domain;

namespace Ordering.Domain.Ids;

public record OrderId : TypedId
{
    public OrderId(Guid id)
    : base(id)
    {
    }
};