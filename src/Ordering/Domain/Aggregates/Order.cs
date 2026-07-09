using Ordering.Domain.Entities;
using Ordering.Domain.Enums;
using Ordering.Domain.Events;
using Ordering.Domain.Ids;
using Ordering.Domain.Policies;
using SharedKernel.Domain;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Domain.Aggregates;

public class Order : AggregateRoot<OrderId>
{
    public OrderId Id { get; }
    public RestaurantRefId RestaurantRefId { get; }

    public OrderStatus Status { get; private set; }
    private Money MinimalPrice { get; set; }

    private readonly List<OrderLine> _orderLines;

    public IReadOnlyList<OrderLine> OrderLines => _orderLines.AsReadOnly();

    public Money? TotalPrice => _orderLines.Count == 0
        ? null
        : _orderLines.Select(l => l.GetTotalPrice()).Aggregate((a, b) => a + b);

    private Order(
        OrderId id, RestaurantRefId restaurantRefId, Money minPrice,
        OrderStatus status = OrderStatus.Draft, List<OrderLine>? list = null
    )
    {
        Id = id;
        RestaurantRefId = restaurantRefId;
        Status = status;
        MinimalPrice = minPrice;
        _orderLines = list ?? [];
    }

    public static Order Create(OrderId id, RestaurantRefId restaurantRefId, Money minPrice)
    {
        var aggregate = new Order(id, restaurantRefId, minPrice);
        return aggregate;
    }

    public static Order Rehydrate(OrderId id, RestaurantRefId restaurantRefId,
        OrderStatus status, List<OrderLine> list, Money minPrice)
    {
        return new Order(id, restaurantRefId, minPrice, status, list);
    }

    public Result<string> Place()
    {
        if (!OrderStatusChangePolicy.CanChangeStatusTo(Status, OrderStatus.Pending))
            return Result<string>.Fail("Can't change order status");
        if (_orderLines.Count == 0) return Result<string>.Fail("No order lines in order");
        if (MinimalPrice.CompareTo(TotalPrice) == 1) return Result<string>.Fail("Order price is too small");

        Status = OrderStatus.Pending;
        AddEvent(new OrderPlaced(Id));
        return Result<string>.Success();
    }

    public Result<string> Confirm()
    {
        if (!OrderStatusChangePolicy.CanChangeStatusTo(Status, OrderStatus.Confirmed))
            return Result<string>.Fail("Can't change order status");
        Status = OrderStatus.Confirmed;

        AddEvent(new OrderConfirmed(Id));
        return Result<string>.Success();
    }

    public Result<string> Cancel()
    {
        if (!OrderStatusChangePolicy.CanChangeStatusTo(Status, OrderStatus.Cancelled))
            return Result<string>.Fail("Can't change order status");
        Status = OrderStatus.Cancelled;

        AddEvent(new OrderCancelled(Id));
        return Result<string>.Success();
    }

    public Result<string> CreateOrderLineItem(
        OrderLineId id, Money newPrice, MenuItemRefId menuItemRefId, byte quantity
    )
    {
        if (Status is not OrderStatus.Draft) return Result<string>.Fail($"Can't change order with status {Status}");

        var orderLine = OrderLine.Create(id, newPrice, menuItemRefId, quantity);
        _orderLines.Add(orderLine);

        return Result<string>.Success();
    }

    public Result<string> AddOrderLineItem(OrderLineId orderLineId)
    {
        if (Status is not OrderStatus.Draft) return Result<string>.Fail($"Can't change order with status {Status}");
        var orderLine = FindOrderLineOrThrow(orderLineId);

        if (orderLine.Quantity >= 255) return Result<string>.Fail("Limit reached");

        orderLine.IncreaseQuantity();
        return Result<string>.Success();
    }

    public Result<string> RemoveOrderLineItem(OrderLineId orderLineId)
    {
        if (Status is not OrderStatus.Draft) return Result<string>.Fail($"Can't change order with status {Status}");

        var orderLine = FindOrderLineOrThrow(orderLineId);

        if (orderLine.Quantity <= 1)
        {
            _orderLines.Remove(orderLine);
            return Result<string>.Success();
        }

        orderLine.DecreaseQuantity();
        return Result<string>.Success();
    }

    public Result<string> ChangeOrderLinePrice(OrderLineId orderLineId, Money newPrice)
    {
        if (Status is not OrderStatus.Draft) return Result<string>.Fail($"Can't change order with status {Status}");

        var orderLine = FindOrderLineOrThrow(orderLineId);

        orderLine.ChangePrice(newPrice);
        return Result<string>.Success();
    }

    public Result<string> ChangeMinimalPrice(Money newMinimalPrice)
    {
        if (Status != OrderStatus.Draft) return Result<string>.Fail($"Can't change order with status {Status}");
        
        MinimalPrice =  newMinimalPrice;
        return Result<string>.Success();
    }

    private OrderLine FindOrderLineOrThrow(OrderLineId id)
    {
        return _orderLines.Find(x => x.Id == id) ?? throw new KeyNotFoundException("Order line not found");
    }
}