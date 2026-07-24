using SharedKernel.Domain;
using Ordering.Domain.Ids;

namespace Ordering.Domain.Events;

public record OrderCancelled(OrderId Id) : DomainEvent<OrderId>(Id);