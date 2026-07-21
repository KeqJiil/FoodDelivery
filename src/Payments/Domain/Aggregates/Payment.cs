using Payments.Domain.Enums;
using Payments.Domain.Events;
using Payments.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Payments.Domain.Aggregates;

public class Payment : AggregateRoot<PaymentId>
{
    public PaymentId Id { get; }
    public OrderRefId OrderRefId { get; }
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? FailureReason { get; private set; }

    private Payment(PaymentId id, OrderRefId orderRefId, Money amount)
    {
        Id = id;
        OrderRefId = orderRefId;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    private Payment()
    {
    }

    public static Payment Create(PaymentId id, OrderRefId orderRefId, Money amount)
    {
        var payment = new Payment(id, orderRefId, amount);
        payment.AddEvent(new PaymentCreated(id));
        return payment;
    }

    public Result<Error> Succeed()
    {
        if (Status != PaymentStatus.Pending)
            return Result<Error>.Fail(Error.Conflict($"Cannot succeed a payment that is already {Status}"));

        Status = PaymentStatus.Succeeded;
        AddEvent(new PaymentSucceeded(Id, OrderRefId, Amount));
        return Result<Error>.Success();
    }

    public Result<Error> Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
            return Result<Error>.Fail(Error.Conflict($"Cannot fail a payment that is already {Status}"));

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        AddEvent(new PaymentFailed(Id, OrderRefId, reason));
        return Result<Error>.Success();
    }
}
