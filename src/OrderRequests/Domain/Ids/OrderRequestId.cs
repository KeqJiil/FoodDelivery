using SharedKernel.Domain;

namespace OrderRequests.Domain.Ids;

public record OrderRequestId : TypedId, IComparable<OrderRequestId>
{
    public OrderRequestId()
    {
    }

    public OrderRequestId(Guid id)
        : base(id)
    {
    }

    public int CompareTo(OrderRequestId? other) => Id.CompareTo(other?.Id ?? Guid.Empty);
};