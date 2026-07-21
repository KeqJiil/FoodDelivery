using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Events;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Domain.Aggregates;

public class OrderRequest : AggregateRoot<OrderRequestId>
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
        var obj = new OrderRequest(id, orderRefId, restaurantRefId);
        obj.AddEvent(new OrderRequested(id));
        return obj;
    }

    public Result<Error> Reject()
    {
        if (Status != OrderRequestStatus.Pending)
            return Result<Error>.Fail(Error.Conflict($"Cannot approve an order request that is already {Status}"));

        Status = OrderRequestStatus.Rejected;
        AddEvent(new OrderRejected(Id));
        return Result<Error>.Success();
    }

    public Result<Error> Approve()
    {
        if (Status != OrderRequestStatus.Pending)
            return Result<Error>.Fail(Error.Conflict($"Cannot approve an order request that is already {Status}"));

        Status = OrderRequestStatus.Approved;
        AddEvent(new OrderApproved(Id, OrderRefId));
        return Result<Error>.Success();
    }
}