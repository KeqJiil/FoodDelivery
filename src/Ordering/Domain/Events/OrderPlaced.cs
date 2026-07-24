using SharedKernel.Domain;
using Ordering.Domain.Ids;

namespace Ordering.Domain.Events;

public record OrderPlaced(OrderId Id, RestaurantRefId RestaurantRefId) : DomainEvent<OrderId>(Id);