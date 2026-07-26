using Payments.Domain.Ids;
using SharedKernel.Domain;

namespace Payments.Domain.Events;

public record PaymentCancelled(PaymentId Id, OrderRefId OrderRefId) : DomainEvent<PaymentId>(Id);