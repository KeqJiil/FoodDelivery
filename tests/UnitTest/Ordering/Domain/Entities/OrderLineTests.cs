using FluentAssertions;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Entities;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Errors;

namespace Ordering.UnitTest.Domain.Entities;

public class OrderLineTests
{
    [Fact]
    public void Create_ShouldFail_WhenQuantityIsZero()
    {
        var orderLineRes = OrderLine.Create(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()), 0);

        orderLineRes.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldSetInitialPriceAndQuantity()
    {
        var orderLineRes = OrderLine.Create(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()), 10);

        var orderLine = orderLineRes.Ok!;

        orderLine.Quantity.Should().Be(10);
        orderLine.Price.Amount.Should().Be(10m);
        orderLine.Price.Currency.Should().Be(Currency.Usd);
    }

    [Fact]
    public void ChangePrice_ShouldUpdatePrice()
    {
        var orderLineRes = OrderLine.Create(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));

        var orderLine = orderLineRes.Ok!;

        orderLine.ChangePrice(Money.Create(Currency.Usd, 5m).Ok!);

        orderLine.Price.Currency.Should().Be(Currency.Usd);
        orderLine.Price.Amount.Should().Be(5m);
    }

    [Fact]
    public void GetTotalPrice_ShouldMultiplyPriceByQuantity()
    {
        var price = 10m;
        byte quantity = 3;
        var orderLineRes = OrderLine.Create(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, price).Ok!,
            new MenuItemRefId(Guid.NewGuid()), quantity);

        var orderLine = orderLineRes.Ok!;

        var totalPrice = orderLine.GetTotalPrice();
        totalPrice.Amount.Should().Be(price * quantity);
    }

    [Fact]
    public void IncreaseQuantity_ShouldAddToQuantity()
    {
        var orderLineRes = OrderLine.Create(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        var orderLine = orderLineRes.Ok!;
        orderLine.IncreaseQuantity(10);

        orderLine.Quantity.Should().Be(11);
    }

    [Fact]
    public void DecreaseQuantity_ShouldSubtractOne()
    {
        var orderLineRes = OrderLine.Create(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()), 11);
        var orderLine = orderLineRes.Ok!;
        orderLine.DecreaseQuantity();
        orderLine.Quantity.Should().Be(10);
    }

    [Fact]
    public void DecreaseQuantity_ShouldBeTrue_WhenQuantityIsNone()
    {
        var orderLineRes = OrderLine.Create(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        orderLineRes.IsSuccess.Should().BeTrue();
        orderLineRes.Ok!.Should().NotBeNull();
    }
}