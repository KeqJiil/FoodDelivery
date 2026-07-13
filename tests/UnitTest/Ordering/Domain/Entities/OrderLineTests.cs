using FluentAssertions;
using Ordering.Domain.Entities;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.UnitTest.Domain.Entities;

public class OrderLineTests
{
    [Fact]
    public void Create_ShouldThrow_WhenQuantityIsZero()
    {
        var fn = () => OrderLine.Create(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()), 0);

        fn.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_ShouldSetInitialPriceAndQuantity()
    {
        var orderLine = OrderLine.Create(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()), 10);

        orderLine.Quantity.Should().Be(10);
        orderLine.Price.Amount.Should().Be(10m);
        orderLine.Price.Currency.Should().Be(Currency.Usd);
    }

    [Fact]
    public void ChangePrice_ShouldUpdatePrice()
    {
        var orderLine = OrderLine.Create(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));

        orderLine.ChangePrice(new Money(Currency.Usd, 5m));

        orderLine.Price.Currency.Should().Be(Currency.Usd);
        orderLine.Price.Amount.Should().Be(5m);
    }

    [Fact]
    public void GetTotalPrice_ShouldMultiplyPriceByQuantity()
    {
        var price = 10m;
        byte quantity = 3;
        var orderLine = OrderLine.Create(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, price),
            new MenuItemRefId(Guid.NewGuid()), quantity);

        var totalPrice = orderLine.GetTotalPrice();
        totalPrice.Amount.Should().Be(price * quantity);
    }

    [Fact]
    public void IncreaseQuantity_ShouldAddToQuantity()
    {
        var orderLine = OrderLine.Create(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        orderLine.IncreaseQuantity(10);

        orderLine.Quantity.Should().Be(11);
    }

    [Fact]
    public void DecreaseQuantity_ShouldSubtractOne()
    {
        var orderLine = OrderLine.Create(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()), 11);
        orderLine.DecreaseQuantity();
        orderLine.Quantity.Should().Be(10);
    }

    [Fact]
    public void DecreaseQuantity_ShouldThrow_WhenQuantityIsOne()
    {
        var orderLine = OrderLine.Create(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));

        var fn = () => orderLine.DecreaseQuantity();
        fn.Should().Throw<InvalidOperationException>();
    }
}