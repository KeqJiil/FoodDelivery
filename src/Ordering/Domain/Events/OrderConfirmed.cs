using SharedKernel.Domain;
using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Ids;

namespace Ordering.Domain.Events;

public class OrderConfirmed(OrderId id, Money amount) : DomainEvent<OrderId>(id)
{
    public Money Amount { get; } = amount;
};