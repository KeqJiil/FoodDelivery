using Payments.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.ValueObjects;

namespace Payments.Domain.Events;

public class PaymentSucceeded(PaymentId id, OrderRefId orderRefId, Money amount) : DomainEvent<PaymentId>(id)
{
    public OrderRefId OrderRefId { get; } = orderRefId;
    public Money Amount { get; } = amount;
}
