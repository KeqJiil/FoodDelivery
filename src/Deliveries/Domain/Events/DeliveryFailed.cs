using Deliveries.Domain.Ids;
using SharedKernel.Domain;

namespace Deliveries.Domain.Events;

public class DeliveryFailed(DeliveryId id, OrderRefId orderRefId, string reason) : DomainEvent<DeliveryId>(id)
{
    public OrderRefId OrderRefId { get; } = orderRefId;
    public string Reason { get; } = reason;
}
