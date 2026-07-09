using Ordering.Domain.Ids;
using SharedKernel.Domain;

namespace Ordering.Domain.Events;

public class OrderCancelled(OrderId id) : DomainEvent<OrderId>(id)
{
    
}
