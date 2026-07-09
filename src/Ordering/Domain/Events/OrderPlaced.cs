using Ordering.Domain.Ids;
using SharedKernel.Domain;

namespace Ordering.Domain.Events;

public class OrderPlaced(OrderId id) : DomainEvent<OrderId>(id)
{

};