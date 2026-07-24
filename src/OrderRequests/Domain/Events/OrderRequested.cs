using OrderRequests.Domain.Ids;
using SharedKernel.Domain;

namespace OrderRequests.Domain.Events;

public record OrderRequested(OrderRequestId Id) : DomainEvent<OrderRequestId>(Id);