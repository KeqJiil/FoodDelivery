using OrderRequests.Domain.Ids;
using SharedKernel.Domain;

namespace OrderRequests.Domain.Events;

public record OrderCancelled(OrderRequestId Id): DomainEvent<OrderRequestId>(Id);