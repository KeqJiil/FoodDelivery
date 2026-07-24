using Deliveries.Domain.Ids;
using SharedKernel.Domain;

namespace Deliveries.Domain.Events;

public record DeliveryPickedUp(DeliveryId id) : DomainEvent<DeliveryId>(id);
