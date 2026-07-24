using Deliveries.Domain.Ids;
using SharedKernel.Domain;

namespace Deliveries.Domain.Events;

public record DeliveryFailed(DeliveryId Id, OrderRefId OrderRefId, string Reason) : DomainEvent<DeliveryId>(Id);