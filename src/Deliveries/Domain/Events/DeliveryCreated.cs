using Deliveries.Domain.Ids;
using SharedKernel.Domain;

namespace Deliveries.Domain.Events;

public class DeliveryCreated(DeliveryId id) : DomainEvent<DeliveryId>(id);
