using SharedKernel.Domain;
using Ordering.Domain.Ids;

namespace Ordering.Domain.Events;

public class OrderCancelled(OrderId id) : DomainEvent<OrderId>(id)
{
}