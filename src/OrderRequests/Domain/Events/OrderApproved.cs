using OrderRequests.Domain.Ids;
using SharedKernel.Domain;

namespace OrderRequests.Domain.Events;

public class OrderApproved(OrderRequestId id, OrderRefId orderRefId) : DomainEvent<OrderRequestId>(id)
{
    public OrderRefId OrderRefId { get; } = orderRefId;
}