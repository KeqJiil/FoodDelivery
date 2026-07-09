using SharedKernel.Domain.Enums;

namespace SharedKernel.Domain.ValueObjects;

public record Money : IComparable<Money>
{
    public Currency Currency { get; }
    public decimal Amount { get; }
    
    public Money(Currency currency, decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 0);
        Currency = currency;
        Amount = amount;
    }

    public Money Add(Money other)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(Currency, other.Currency);
        return new Money(Currency, Amount + other.Amount);
    }
    
    public Money Subtract(Money other)
    {
        var newAmount = Amount - other.Amount;
        if (newAmount < 0 || Currency != other.Currency) throw new ArgumentOutOfRangeException();
        return new Money(Currency, newAmount);
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
        return new Money(money.Currency, money.Amount * amount);
    }
}