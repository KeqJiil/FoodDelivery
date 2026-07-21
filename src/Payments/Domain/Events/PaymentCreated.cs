using Payments.Domain.Ids;
using SharedKernel.Domain;

namespace Payments.Domain.Events;

public class PaymentCreated(PaymentId id) : DomainEvent<PaymentId>(id);
