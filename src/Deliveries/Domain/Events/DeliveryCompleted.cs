using Deliveries.Domain.Ids;
using SharedKernel.Domain;

namespace Deliveries.Domain.Events;

public class DeliveryCompleted(DeliveryId id, OrderRefId orderRefId) : DomainEvent<DeliveryId>(id)
{
    public OrderRefId OrderRefId { get; } = orderRefId;
}
