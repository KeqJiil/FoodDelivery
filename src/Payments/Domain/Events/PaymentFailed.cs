using Payments.Domain.Ids;
using SharedKernel.Domain;

namespace Payments.Domain.Events;

public class PaymentFailed(PaymentId id, OrderRefId orderRefId, string reason) : DomainEvent<PaymentId>(id)
{
    public OrderRefId OrderRefId { get; } = orderRefId;
    public string Reason { get; } = reason;
}
