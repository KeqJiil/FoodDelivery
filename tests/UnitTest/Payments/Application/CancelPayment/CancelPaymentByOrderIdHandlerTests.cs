using FluentAssertions;
using Moq;
using Payments.Application.Abstractions;
using Payments.Application.CancelPayment;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Payments.Domain.Events;
using Payments.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Payments.UnitTest.Application.CancelPayment;

public class CancelPaymentByOrderIdHandlerTests
{
    private readonly Mock<IPaymentRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly CancelPaymentByOrderIdHandler _handler;

    public CancelPaymentByOrderIdHandlerTests()
    {
        _handler = new CancelPaymentByOrderIdHandler(_repository.Object, _unitOfWork.Object);
    }

    private static Payment PendingPayment(Guid orderId) =>
        Payment.Create(new PaymentId(Guid.NewGuid()), new OrderRefId(orderId),
            Money.Create(Currency.Usd, 42m).Ok!);

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenNoPaymentExistsForOrder()
    {
        _repository.Setup(r => r.GetByOrderIdAsync(It.IsAny<OrderRefId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _handler.Handle(new CancelPaymentByOrderIdCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldLookUpPayment_ByOrderIdFromCommand()
    {
        var orderId = Guid.NewGuid();
        _repository.Setup(r => r.GetByOrderIdAsync(It.IsAny<OrderRefId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PendingPayment(orderId));

        await _handler.Handle(new CancelPaymentByOrderIdCommand(orderId), CancellationToken.None);

        _repository.Verify(r => r.GetByOrderIdAsync(new OrderRefId(orderId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelPayment_AndPersist_WhenPending()
    {
        var orderId = Guid.NewGuid();
        var payment = PendingPayment(orderId);
        _repository.Setup(r => r.GetByOrderIdAsync(new OrderRefId(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _handler.Handle(new CancelPaymentByOrderIdCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Cancelled);
        payment.Events.Should().ContainSingle(e => e is PaymentCancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelPayment_WhenPaymentAlreadyFailed()
    {
        var orderId = Guid.NewGuid();
        var payment = PendingPayment(orderId);
        payment.Fail("Gateway declined");
        _repository.Setup(r => r.GetByOrderIdAsync(new OrderRefId(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _handler.Handle(new CancelPaymentByOrderIdCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Cancelled);
        payment.FailureReason.Should().Be("Gateway declined");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFailWithConflict_WhenPaymentAlreadySucceeded()
    {
        var orderId = Guid.NewGuid();
        var payment = PendingPayment(orderId);
        payment.Succeed();
        _repository.Setup(r => r.GetByOrderIdAsync(new OrderRefId(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _handler.Handle(new CancelPaymentByOrderIdCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.Events.Should().NotContain(e => e is PaymentCancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFailWithConflict_WhenPaymentAlreadyCancelled()
    {
        var orderId = Guid.NewGuid();
        var payment = PendingPayment(orderId);
        payment.Cancel();
        _repository.Setup(r => r.GetByOrderIdAsync(new OrderRefId(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _handler.Handle(new CancelPaymentByOrderIdCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        payment.Events.Should().ContainSingle(e => e is PaymentCancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPropagateCancellationToken_ToRepositoryAndUnitOfWork()
    {
        var orderId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        _repository.Setup(r => r.GetByOrderIdAsync(new OrderRefId(orderId), cts.Token))
            .ReturnsAsync(PendingPayment(orderId));

        await _handler.Handle(new CancelPaymentByOrderIdCommand(orderId), cts.Token);

        _repository.Verify(r => r.GetByOrderIdAsync(new OrderRefId(orderId), cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }
}
