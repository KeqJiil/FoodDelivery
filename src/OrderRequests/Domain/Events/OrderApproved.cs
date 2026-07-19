using OrderRequests.Domain.Ids;
using SharedKernel.Domain;

namespace OrderRequests.Domain.Events;

public class OrderApproved(OrderRequestId Id) : DomainEvent<OrderRequestId>(Id);