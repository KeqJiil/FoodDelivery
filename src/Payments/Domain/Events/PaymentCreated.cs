using Payments.Domain.Ids;
using SharedKernel.Domain;

namespace Payments.Domain.Events;

public record PaymentCreated(PaymentId Id) : DomainEvent<PaymentId>(Id);
