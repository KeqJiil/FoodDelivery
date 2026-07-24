using Payments.Domain.Ids;
using SharedKernel.Domain;

namespace Payments.Domain.Events;

public record PaymentFailed(PaymentId Id, OrderRefId OrderRefId, string Reason) : DomainEvent<PaymentId>(Id);
