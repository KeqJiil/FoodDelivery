using OrderRequests.Domain.Ids;
using SharedKernel.Domain;

namespace OrderRequests.Domain.Events;

public class OrderRejected(OrderRequestId Id) : DomainEvent<OrderRequestId>(Id);