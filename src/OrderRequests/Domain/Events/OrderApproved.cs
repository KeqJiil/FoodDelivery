using OrderRequests.Domain.Ids;
using SharedKernel.Domain;

namespace OrderRequests.Domain.Events;

public record OrderApproved(OrderRequestId Id, OrderRefId OrderRefId) : DomainEvent<OrderRequestId>(Id);