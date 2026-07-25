using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Domain.Events;

public record OrderFailed(OrderId Id) : DomainEvent<OrderId>(Id);