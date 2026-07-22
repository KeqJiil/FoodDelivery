using FluentAssertions;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Payments.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Payments.UnitTest.Domain.Aggregates;

public class PaymentTests
{
    private static Money Amount(decimal value = 10m) => Money.Create(Currency.Usd, value).Ok!;

    [Fact]
    public void Create_ShouldReturnPendingPayment_WithGivenIds()
    {
        var id = new PaymentId(Guid.NewGuid());
        var orderRefId = new OrderRefId(Guid.NewGuid());
        var amount = Amount();

        var payment = Payment.Create(id, orderRefId, amount);

        payment.Id.Should().Be(id);
        payment.OrderRefId.Should().Be(orderRefId);
        payment.Amount.Should().Be(amount);
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Succeed_ShouldTransitionToSucceeded_WhenPending()
    {
        var payment = Payment.Create(new PaymentId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()), Amount());

        var result = payment.Succeed();

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public void Succeed_ShouldFail_WhenStatusIsNotPending()
    {
        var payment = Payment.Create(new PaymentId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()), Amount());
        payment.Succeed();

        var result = payment.Succeed();

        result.IsSuccess.Should().BeFalse();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public void Fail_ShouldTransitionToFailed_AndStoreReason_WhenPending()
    {
        var payment = Payment.Create(new PaymentId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()), Amount());

        var result = payment.Fail("Insufficient funds");

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Insufficient funds");
    }

    [Fact]
    public void Fail_ShouldFail_WhenAlreadySucceeded()
    {
        var payment = Payment.Create(new PaymentId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()), Amount());
        payment.Succeed();

        var result = payment.Fail("Too late");

        result.IsSuccess.Should().BeFalse();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
    }
}
