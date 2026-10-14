using SharedKernel.Domain;

namespace Ordering.Domain.Ids;

public record OrderId : TypedId, IComparable<OrderId>
{
    public OrderId()
    {
    }

    public OrderId(Guid id)
        : base(id)
    {
    }

    public int CompareTo(OrderId? other) => other is null ? 1 : Id.CompareTo(other.Id);

    public static bool operator >(OrderId left, OrderId right) => left.CompareTo(right) > 0;

    public static bool operator <(OrderId left, OrderId right) => left.CompareTo(right) < 0;
};
