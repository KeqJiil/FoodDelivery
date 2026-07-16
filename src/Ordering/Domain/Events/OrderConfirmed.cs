using SharedKernel.Domain;
using Ordering.Domain.Ids;

namespace Ordering.Domain.Events;

public class OrderConfirmed(OrderId id) : DomainEvent<OrderId>(id)
{
};