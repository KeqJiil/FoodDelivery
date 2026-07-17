using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace SharedKernel.Domain.ValueObjects;

public record Money : IComparable<Money>
{
    public Currency Currency { get; }
    public decimal Amount { get; }

    private Money(Currency currency, decimal amount)
    {
        Currency = currency;
        Amount = amount;
    }

    public static Result<Money, Error> Create(Currency currency, decimal amount)
    {
        return amount < 0
            ? Result<Money, Error>.Fail(Error.Validation("Amount can't be negative"))
            : Result<Money, Error>.Success(new Money(currency, amount));
    }

    public static Money operator +(Money current, Money other)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(current.Currency, other.Currency);
        return new Money(current.Currency, current.Amount + other.Amount);
    }

    public static Money operator -(Money current, Money other)
    {
        var newAmount = current.Amount - other.Amount;
        if (newAmount < 0 || current.Currency != other.Currency) throw new ArgumentOutOfRangeException();
        return new Money(current.Currency, newAmount);
    }

    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        ArgumentOutOfRangeException.ThrowIfNotEqual(Currency, other.Currency);
        var difference = Amount - other.Amount;
        if (difference == 0) return 0;
        return difference > 0 ? 1 : -1;
    }

    public static Money operator *(Money money, decimal amount)
    {
        var newAmount = money.Amount * amount;
        ArgumentOutOfRangeException.ThrowIfLessThan(newAmount, 0);
        return new Money(money.Currency, newAmount);
    }
}