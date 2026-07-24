using Payments.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.ValueObjects;

namespace Payments.Domain.Events;

public record PaymentSucceeded(PaymentId Id, OrderRefId OrderRefId) : DomainEvent<PaymentId>(Id);