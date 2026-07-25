using Deliveries.Domain.Ids;
using SharedKernel.Domain;

namespace Deliveries.Domain.Events;

public record DeliveryCreated(DeliveryId Id, OrderRefId OrderRefId) : DomainEvent<DeliveryId>(Id);