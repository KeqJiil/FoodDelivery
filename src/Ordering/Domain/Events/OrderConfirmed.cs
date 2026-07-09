using Ordering.Domain.Ids;
using SharedKernel.Domain;

namespace Ordering.Domain.Events;

public class OrderConfirmed(OrderId id) : DomainEvent<OrderId>(id)
{

};