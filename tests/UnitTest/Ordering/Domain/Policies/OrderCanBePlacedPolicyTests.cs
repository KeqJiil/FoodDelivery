using FluentAssertions;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using Ordering.Domain.Policies;

namespace Ordering.UnitTest.Domain.Policies;

public class OrderCanBePlacedPolicyTests
{
    [Fact]
    public void CanBePlaced_ShouldFail_WhenStatusTransitionNotAllowed()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Eur, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        var money = Money.Create(Currency.Eur, 2m).Ok!;
        order.Place(money);

        var result = OrderCanBePlacedPolicy.CanBePlaced(order, money);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
    }

    [Fact]
    public void CanBePlaced_ShouldFail_WhenNoOrderLines()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var money = Money.Create(Currency.Eur, 2m).Ok!;

        var result = OrderCanBePlacedPolicy.CanBePlaced(order, money);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Validation);
    }

    [Fact]
    public void CanBePlaced_ShouldFail_WhenTotalPriceBelowMinimum()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Eur, 1m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        var money = Money.Create(Currency.Eur, 2m).Ok!;

        var result = OrderCanBePlacedPolicy.CanBePlaced(order, money);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Validation);
    }

    [Fact]
    public void CanBePlaced_ShouldSucceed_WhenTotalPriceMeetsMinimum()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Eur, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        var money = Money.Create(Currency.Eur, 2m).Ok!;

        var result = OrderCanBePlacedPolicy.CanBePlaced(order, money);

        result.IsSuccess.Should().BeTrue();
    }
}