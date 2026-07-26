using SharedKernel.Domain;
using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Ids;

namespace Ordering.Domain.Events;

public record OrderConfirmed(OrderId Id) : DomainEvent<OrderId>(Id);