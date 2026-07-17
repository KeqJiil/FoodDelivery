using FluentAssertions;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace SharedKernel.UnitTest.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_ShouldFail_WhenAmountIsNegative()
    {
        var result = Money.Create(Currency.Eur, -1m);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Add_ShouldSumAmounts_WhenSameCurrency()
    {
        var money1 = Money.Create(Currency.Eur, 1m).Ok!;
        var money2 = Money.Create(Currency.Eur, 1m).Ok!;
        var result = money1 + money2;
        result.Amount.Should().Be(2m);
    }

    [Fact]
    public void Add_ShouldThrow_WhenCurrenciesDiffer()
    {
        var money1 = Money.Create(Currency.Eur, 1m).Ok!;
        var money2 = Money.Create(Currency.Usd, 1m).Ok!;
        var fn = () => money2 + money1;
        fn.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Subtract_ShouldSubtractAmounts_WhenSameCurrency()
    {
        var money1 = Money.Create(Currency.Eur, 2m).Ok!;
        var money2 = Money.Create(Currency.Eur, 1m).Ok!;
        var result = money1 - money2;
        result.Amount.Should().Be(1m);
    }

    [Fact]
    public void Subtract_ShouldThrow_WhenResultWouldBeNegative()
    {
        var money1 = Money.Create(Currency.Eur, 5m).Ok!;
        var money2 = Money.Create(Currency.Eur, 2m).Ok!;
        var fn = () => money2 - money1;
        fn.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Subtract_ShouldThrow_WhenCurrenciesDiffer()
    {
        var money1 = Money.Create(Currency.Eur, 5m).Ok!;
        var money2 = Money.Create(Currency.Usd, 10m).Ok!;
        var fn = () => money2 - money1;
        fn.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CompareTo_ShouldReflectAmountDifference_WhenSameCurrency()
    {
        var money1 = Money.Create(Currency.Eur, 5m).Ok!;
        var money2 = Money.Create(Currency.Eur, 2m).Ok!;
        var fn = () => money1.CompareTo(money2);
        fn.Should().NotThrow();
        fn().Should().Be(1);
    }

    [Fact]
    public void CompareTo_ShouldThrow_WhenCurrenciesDiffer()
    {
        var money1 = Money.Create(Currency.Eur, 5m).Ok!;
        var money2 = Money.Create(Currency.Usd, 2m).Ok!;
        var fn = () => money1.CompareTo(money2);
        fn.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Multiply_ShouldScaleAmount()
    {
        var money1 = Money.Create(Currency.Eur, 5m).Ok!;
        var newMoney = money1 * 10;
        newMoney.Amount.Should().Be(50m);
    }
}
