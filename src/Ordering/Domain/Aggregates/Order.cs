using Ordering.Domain.Entities;
using Ordering.Domain.Enums;
using Ordering.Domain.Events;
using Ordering.Domain.Ids;
using Ordering.Domain.Policies;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Domain.Aggregates;

public class Order : AggregateRoot<OrderId>
{
    public OrderId Id { get; }
    public RestaurantRefId RestaurantRefId { get; }

    public OrderStatus Status { get; private set; }

    private readonly List<OrderLine> _orderLines;

    public IReadOnlyList<OrderLine> OrderLines => _orderLines.AsReadOnly();

    public Money? TotalPrice => _orderLines.Count == 0
        ? null
        : _orderLines.Select(l => l.GetTotalPrice()).Aggregate((a, b) => a + b);

    private Order(
        OrderId id, RestaurantRefId restaurantRefId,
        OrderStatus status = OrderStatus.Draft, List<OrderLine>? list = null
    )
    {
        Id = id;
        RestaurantRefId = restaurantRefId;
        Status = status;
        _orderLines = list ?? [];
    }

    public static Order Create(OrderId id, RestaurantRefId restaurantRefId)
    {
        var aggregate = new Order(id, restaurantRefId);
        return aggregate;
    }

    public static Order Rehydrate(OrderId id, RestaurantRefId restaurantRefId,
        OrderStatus status, List<OrderLine> list)
    {
        return new Order(id, restaurantRefId, status, list);
    }

    public Result<Error> Place(Money minimalPrice)
    {
        var policyResult = OrderCanBePlacedPolicy.CanBePlaced(this, minimalPrice);
        if (!policyResult.IsSuccess)
            return Result<Error>.Fail(policyResult.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected Error"));

        Status = OrderStatus.Pending;
        AddEvent(new OrderPlaced(Id));
        return Result<Error>.Success();
    }

    public Result<Error> Confirm()
    {
        if (!OrderStatusChangePolicy.CanChangeStatusTo(Status, OrderStatus.Confirmed))
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Status can't be changed"));
        Status = OrderStatus.Confirmed;

        AddEvent(new OrderConfirmed(Id));
        return Result<Error>.Success();
    }

    public Result<Error> Cancel()
    {
        if (!OrderStatusChangePolicy.CanChangeStatusTo(Status, OrderStatus.Cancelled))
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Can't change order status"));
        Status = OrderStatus.Cancelled;

        AddEvent(new OrderCancelled(Id));
        return Result<Error>.Success();
    }

    public Result<Error> CreateOrderLineItem(
        OrderLineId id, Money newPrice, MenuItemRefId menuItemRefId, byte quantity
    )
    {
        if (Status is not OrderStatus.Draft)
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Status can't be changed"));

        var orderLine = OrderLine.Create(id, newPrice, menuItemRefId, quantity);
        _orderLines.Add(orderLine);

        return Result<Error>.Success();
    }

    public Result<Error> AddOrderLineItem(OrderLineId orderLineId)
    {
        if (Status is not OrderStatus.Draft)
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Status can't be changed"));
        var orderLine = FindOrderLineOrThrow(orderLineId);

        if (orderLine.Quantity >= 255)
            return Result<Error>.Fail(new Error(ErrorEnum.Validation, "Quantity can't be greater than 255"));

        orderLine.IncreaseQuantity();
        return Result<Error>.Success();
    }

    public Result<Error> RemoveOrderLineItem(OrderLineId orderLineId)
    {
        if (Status is not OrderStatus.Draft)
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Status can't be changed"));

        var orderLine = FindOrderLineOrThrow(orderLineId);

        if (orderLine.Quantity <= 1)
        {
            _orderLines.Remove(orderLine);
            return Result<Error>.Success();
        }

        orderLine.DecreaseQuantity();
        return Result<Error>.Success();
    }

    public Result<Error> ChangeOrderLinePrice(OrderLineId orderLineId, Money newPrice)
    {
        if (Status is not OrderStatus.Draft)
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Status can't be changed"));

        var orderLine = FindOrderLineOrThrow(orderLineId);

        orderLine.ChangePrice(newPrice);
        return Result<Error>.Success();
    }

    private OrderLine FindOrderLineOrThrow(OrderLineId id)
    {
        return _orderLines.Find(x => x.Id == id) ?? throw new KeyNotFoundException("Order line not found");
    }
}