using FluentAssertions;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace SharedKernel.UnitTest.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenAmountIsNegative()
    {
        var fn = () => new Money(Currency.Eur, -1m);
        fn.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Add_ShouldSumAmounts_WhenSameCurrency()
    {
        var money1 = new Money(Currency.Eur, 1m);
        var money2 = new Money(Currency.Eur, 1m);
        var result = money1 + money2;
        result.Amount.Should().Be(2m);
    }

    [Fact]
    public void Add_ShouldThrow_WhenCurrenciesDiffer()
    {
        var money1 = new Money(Currency.Eur, 1m);
        var money2 = new Money(Currency.Usd, 1m);
        var fn = () => money2 + money1;
        fn.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Subtract_ShouldSubtractAmounts_WhenSameCurrency()
    {
        var money1 = new Money(Currency.Eur, 2m);
        var money2 = new Money(Currency.Eur, 1m);
        var result = money1 - money2;
        result.Amount.Should().Be(1m);
    }

    [Fact]
    public void Subtract_ShouldThrow_WhenResultWouldBeNegative()
    {
        var money1 = new Money(Currency.Eur, 5m);
        var money2 = new Money(Currency.Eur, 2m);
        var fn = () => money2 - money1;
        fn.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Subtract_ShouldThrow_WhenCurrenciesDiffer()
    {
        var money1 = new Money(Currency.Eur, 5m);
        var money2 = new Money(Currency.Usd, 10m);
        var fn = () => money2 - money1;
        fn.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CompareTo_ShouldReflectAmountDifference_WhenSameCurrency()
    {
        var money1 = new Money(Currency.Eur, 5m);
        var money2 = new Money(Currency.Eur, 2m);
        var fn = () => money1.CompareTo(money2);
        fn.Should().NotThrow();
        fn().Should().Be(1);
    }

    [Fact]
    public void CompareTo_ShouldThrow_WhenCurrenciesDiffer()
    {
        var money1 = new Money(Currency.Eur, 5m);
        var money2 = new Money(Currency.Usd, 2m);
        var fn = () => money1.CompareTo(money2);
        fn.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Multiply_ShouldScaleAmount()
    {
        var money1 = new Money(Currency.Eur, 5m);
        var newMoney = money1 * 10;
        newMoney.Amount.Should().Be(50m);
    }
}