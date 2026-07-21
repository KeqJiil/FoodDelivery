using SharedKernel.Domain;
using Ordering.Domain.Ids;

namespace Ordering.Domain.Events;

public class OrderPlaced(OrderId id, RestaurantRefId restaurantRefId) : DomainEvent<OrderId>(id)
{
    public RestaurantRefId RestaurantRefId { get; } = restaurantRefId;
};