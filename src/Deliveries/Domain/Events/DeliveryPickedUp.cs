using Deliveries.Domain.Ids;
using SharedKernel.Domain;

namespace Deliveries.Domain.Events;

public class DeliveryPickedUp(DeliveryId id) : DomainEvent<DeliveryId>(id);
