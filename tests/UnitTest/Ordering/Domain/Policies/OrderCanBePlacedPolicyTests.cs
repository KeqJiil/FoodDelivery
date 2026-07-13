using FluentAssertions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using Ordering.Domain.Policies;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.UnitTest.Domain.Policies;

public class OrderCanBePlacedPolicyTests
{
    [Fact]
    public void CanBePlaced_ShouldFail_WhenStatusTransitionNotAllowed()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Eur, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        var money = new Money(Currency.Eur, 2m);
        order.Place(money);

        var result = OrderCanBePlacedPolicy.CanBePlaced(order, money);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
    }

    [Fact]
    public void CanBePlaced_ShouldFail_WhenNoOrderLines()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var money = new Money(Currency.Eur, 2m);

        var result = OrderCanBePlacedPolicy.CanBePlaced(order, money);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Validation);
    }

    [Fact]
    public void CanBePlaced_ShouldFail_WhenTotalPriceBelowMinimum()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Eur, 1m),
            new MenuItemRefId(Guid.NewGuid()));
        var money = new Money(Currency.Eur, 2m);

        var result = OrderCanBePlacedPolicy.CanBePlaced(order, money);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Validation);
    }

    [Fact]
    public void CanBePlaced_ShouldSucceed_WhenTotalPriceMeetsMinimum()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Eur, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        var money = new Money(Currency.Eur, 2m);

        var result = OrderCanBePlacedPolicy.CanBePlaced(order, money);

        result.IsSuccess.Should().BeTrue();
    }
}