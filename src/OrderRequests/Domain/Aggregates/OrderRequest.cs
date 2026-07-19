using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Domain.Aggregates;

public class OrderRequest
{
    public OrderRequestId Id { get; }
    public OrderRefId OrderRefId { get; }
    public RestaurantRefId RestaurantRefId { get; }
    public OrderRequestStatus Status { get; private set; }

    private OrderRequest(OrderRequestId id, OrderRefId orderRefId, RestaurantRefId restaurantRefId)
    {
        Id = id;
        OrderRefId = orderRefId;
        RestaurantRefId = restaurantRefId;
        Status = OrderRequestStatus.Pending;
    }

    private OrderRequest()
    {
    }

    public static OrderRequest Create(OrderRequestId id, OrderRefId orderRefId, RestaurantRefId restaurantRefId)
    {
        return new OrderRequest(id, orderRefId, restaurantRefId);
    }

    public Result<Error> Reject()
    {
        if (Status != OrderRequestStatus.Pending) return Result<Error>.Fail(Error.Conflict("Wrong status"));

        Status = OrderRequestStatus.Rejected;
        return Result<Error>.Success();
    }

    public Result<Error> Approve()
    {
        if (Status != OrderRequestStatus.Pending) return Result<Error>.Fail(Error.Conflict("Wrong status"));

        Status = OrderRequestStatus.Approved;
        return Result<Error>.Success();
    }
}