using OrderRequests.Domain.Ids;
using SharedKernel.Domain;

namespace OrderRequests.Domain.Events;

public class OrderRequested(OrderRequestId Id) : DomainEvent<OrderRequestId>(Id);