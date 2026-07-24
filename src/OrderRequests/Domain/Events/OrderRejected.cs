using OrderRequests.Domain.Ids;
using SharedKernel.Domain;

namespace OrderRequests.Domain.Events;

public record OrderRejected(OrderRequestId Id) : DomainEvent<OrderRequestId>(Id);