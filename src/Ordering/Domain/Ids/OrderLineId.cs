using SharedKernel.Domain;

namespace Ordering.Domain.Ids;

public record OrderLineId : TypedId
{
    public OrderLineId(Guid id)
        : base(id)
    {
        
    }
}