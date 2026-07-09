using Ordering.Domain.Ids;
using SharedKernel.Domain;

namespace Ordering.Domain.Events;

public class OrderCompleted(OrderId id) : DomainEvent<OrderId>(id)
{

};