using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;

namespace Ordering.Domain.Policies;

public class OrderStatusChangePolicy
{
    public static bool CanChangeStatusTo(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Draft when newStatus is OrderStatus.Pending or OrderStatus.Cancelled => true,
            OrderStatus.Pending when newStatus is OrderStatus.Cancelled or OrderStatus.Processing or OrderStatus.Failed
                => true,
            OrderStatus.Processing when newStatus is OrderStatus.Confirmed or OrderStatus.Failed => true,
            _ => false
        };
    }
}