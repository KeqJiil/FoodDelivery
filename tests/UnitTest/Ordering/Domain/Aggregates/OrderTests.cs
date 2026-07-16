using FluentAssertions;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;

namespace Ordering.UnitTest.Domain.Aggregates;

public class OrderTests
{
    [Fact]
    public void Create_ShouldReturnDraftOrder_WithNoLines()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));

        order.OrderLines.Count.Should().Be(0);
        order.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact]
    public void Place_ShouldTransitionToPending_AndRaiseOrderPlaced_WhenPolicyAllows()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        var result = order.Place(new Money(Currency.Usd, 1m));

        order.Status.Should().Be(OrderStatus.Pending);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Place_ShouldFail_WhenPolicyRejects()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        var result = order.Place(new Money(Currency.Usd, 15m));

        order.Status.Should().Be(OrderStatus.Draft);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Confirm_ShouldTransitionToConfirmed_AndRaiseOrderConfirmed_WhenPending()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        order.Place(new Money(Currency.Usd, 1m));

        order.Status.Should().Be(OrderStatus.Pending);

        order.Confirm();
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_ShouldFail_WhenStatusIsNotPending()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        var result = order.Confirm();

        order.Status.Should().Be(OrderStatus.Draft);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ShouldTransitionToCancelled_AndRaiseOrderCancelled_WhenAllowed()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        var result = order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Cancel_ShouldFail_WhenStatusDisallowsTransition()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        order.Place(new Money(Currency.Usd, 1m));
        order.Confirm();
        var result = order.Cancel();

        order.Status.Should().Be(OrderStatus.Confirmed);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AddOrderLineItem_ShouldCreateNewLine_WhenMenuItemNotPresent()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var before = order.OrderLines.Count;
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        var after = order.OrderLines.Count;

        before.Should().NotBe(after);
        after.Should().Be(before + 1);
    }

    [Fact]
    public void AddOrderLineItem_ShouldIncreaseQuantity_WhenMenuItemAlreadyPresent()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var menuItemId = new MenuItemRefId(Guid.NewGuid());
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), menuItemId);
        var before = order.OrderLines.Count;
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), menuItemId);
        var after = order.OrderLines.Count;

        before.Should().Be(after);

        var item = order.OrderLines[0];
        item.Quantity.Should().Be(2);
    }

    [Fact]
    public void AddOrderLineItem_ShouldFail_WhenQuantityWouldExceed255()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var menuId = new MenuItemRefId(Guid.NewGuid());
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), menuId, 250);
        var result = order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), menuId, 6);

        result.IsSuccess.Should().BeFalse();
        order.OrderLines[0].Quantity.Should().Be(250);
    }

    [Fact]
    public void AddOrderLineItem_ShouldFail_WhenStatusIsNotDraft()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        order.Place(new Money(Currency.Usd, 1m));

        var result = order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void RemoveOrderLineItem_ShouldRemoveLine_WhenQuantityIsOne()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var orderLineId = new OrderLineId(Guid.NewGuid());
        order.AddOrderLineItem(orderLineId, new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()));
        var before = order.OrderLines.Count;
        var result = order.RemoveOrderLineItem(orderLineId);
        var after = order.OrderLines.Count;

        before.Should().NotBe(after);
        before.Should().Be(after + 1);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RemoveOrderLineItem_ShouldDecreaseQuantity_WhenQuantityGreaterThanOne()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var orderLineId = new OrderLineId(Guid.NewGuid());
        order.AddOrderLineItem(orderLineId, new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()), 2);

        var result = order.RemoveOrderLineItem(orderLineId);

        result.IsSuccess.Should().BeTrue();
        order.OrderLines[0].Quantity.Should().Be(1);
    }

    [Fact]
    public void RemoveOrderLineItem_ShouldSucceed_WhenLineDoesNotExist()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var result = order.RemoveOrderLineItem(new OrderLineId(Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RemoveOrderLineItem_ShouldFail_WhenStatusIsNotDraft()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var orderLineId = new OrderLineId(Guid.NewGuid());
        order.AddOrderLineItem(orderLineId, new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()));
        order.Place(new Money(Currency.Usd, 1m));
        var before = order.OrderLines.Count;
        var result = order.RemoveOrderLineItem(orderLineId);
        var after = order.OrderLines.Count;

        before.Should().Be(after);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ChangeOrderLinePrice_ShouldUpdatePrice_WhenLineExistsAndDraft()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var menuId = new MenuItemRefId(Guid.NewGuid());
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), menuId, 2);
        var result = order.ChangeOrderLinePrice(menuId, new Money(Currency.Usd, 20m));

        result.IsSuccess.Should().BeTrue();
        order.OrderLines[0].Price.Amount.Should().Be(20m);
    }

    [Fact]
    public void ChangeOrderLinePrice_ShouldFail_WhenLineNotFound()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var menuId = new MenuItemRefId(Guid.NewGuid());
        var result = order.ChangeOrderLinePrice(menuId, new Money(Currency.Usd, 20m));

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ChangeOrderLinePrice_ShouldFail_WhenStatusIsNotDraft()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var menuId = new MenuItemRefId(Guid.NewGuid());
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), menuId);
        order.Place(new Money(Currency.Usd, 1m));

        var result = order.ChangeOrderLinePrice(menuId, new Money(Currency.Usd, 20m));

        result.IsSuccess.Should().BeFalse();
        order.OrderLines[0].Price.Amount.Should().Be(10m);
    }

    [Fact]
    public void TotalPrice_ShouldBeNull_WhenNoLines()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var totalPrice = order.TotalPrice;

        totalPrice.Should().BeNull();
    }

    [Fact]
    public void TotalPrice_ShouldSumAllLines_WhenLinesExist()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var prices = new[] { 10m, 15m, 6m, 124m };
        var sum = 0m;
        foreach (var price in prices)
        {
            order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, price),
                new MenuItemRefId(Guid.NewGuid()));
            sum += price;
        }

        var totalPrice = order.TotalPrice;

        sum.Should().Be(totalPrice!.Amount);
        order.OrderLines.Count.Should().Be(prices.Length);
    }
}