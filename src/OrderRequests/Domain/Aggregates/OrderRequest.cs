using OrderRequests.Domain.Ids;

namespace OrderRequests.Domain.Aggregates;

public class OrderRequest
{
    public OrderRequestId Id { get; }
    public OrderRefId OrderRefId { get; }
    public RestaurantRefId RestaurantRefId { get; }

    private OrderRequest(OrderRequestId id, OrderRefId orderRefId, RestaurantRefId restaurantRefId)
    {
        Id = id;
        OrderRefId = orderRefId;
        RestaurantRefId = restaurantRefId;
    }
}