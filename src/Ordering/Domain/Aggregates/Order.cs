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

    private Order(OrderId id, RestaurantRefId restaurantRefId, OrderStatus status = OrderStatus.Draft)
    {
        Id = id;
        RestaurantRefId = restaurantRefId;
        Status = status;
        _orderLines = [];
    }

    public static Order Create(OrderId id, RestaurantRefId restaurantRefId)
    {
        var aggregate = new Order(id, restaurantRefId);
        return aggregate;
    }

    public static Order Rehydrate(OrderId id, RestaurantRefId restaurantRefId,
        OrderStatus status, List<OrderLine> list)
    {
        var order = new Order(id, restaurantRefId, status);
        order._orderLines.AddRange(list);
        return order;
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

    public Result<Error> AddOrderLineItem(OrderLineId orderLineId, Money newPrice, MenuItemRefId menuItemRefId,
        byte quantity = 1)
    {
        if (Status is not OrderStatus.Draft)
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Status can't be changed"));
        var orderLine = FindOrderLineByMenuItem(menuItemRefId);

        if (orderLine is null)
        {
            var orderLineNewItem = OrderLine.Create(orderLineId, newPrice, menuItemRefId, quantity);
            _orderLines.Add(orderLineNewItem);
            return Result<Error>.Success();
        }

        if (orderLine.Quantity >= 255)
            return Result<Error>.Fail(new Error(ErrorEnum.Validation, "Quantity can't be greater than 255"));

        orderLine.IncreaseQuantity(quantity);
        return Result<Error>.Success();
    }

    public Result<Error> RemoveOrderLineItem(OrderLineId orderLineId)
    {
        if (Status is not OrderStatus.Draft)
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Status can't be changed"));

        var orderLine = FindOrderLineOrThrow(orderLineId);
        if (orderLine is null) return Result<Error>.Success();

        if (orderLine.Quantity <= 1)
        {
            _orderLines.Remove(orderLine);
            return Result<Error>.Success();
        }

        orderLine.DecreaseQuantity();
        return Result<Error>.Success();
    }

    public Result<Error> ChangeOrderLinePrice(MenuItemRefId menuItemRefId, Money newPrice)
    {
        if (Status is not OrderStatus.Draft)
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Price can't be changed"));

        var orderLine = FindOrderLineByMenuItem(menuItemRefId);
        if (orderLine is null) return Result<Error>.Fail(new Error(ErrorEnum.NotFound, "Order line not found"));

        orderLine.ChangePrice(newPrice);
        return Result<Error>.Success();
    }

    private OrderLine? FindOrderLineOrThrow(OrderLineId id)
    {
        return _orderLines.Find(x => x.Id == id);
    }

    private OrderLine? FindOrderLineByMenuItem(MenuItemRefId menuItemRefId)
    {
        return _orderLines.Find(x => x.MenuItemRefId == menuItemRefId);
    }
}