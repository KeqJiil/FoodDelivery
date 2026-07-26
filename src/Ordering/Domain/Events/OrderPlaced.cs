using SharedKernel.Domain;
using Ordering.Domain.Ids;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Domain.Events;

public record OrderPlaced(OrderId Id, RestaurantRefId RestaurantRefId, Money Price) : DomainEvent<OrderId>(Id);