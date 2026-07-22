using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Payments.Application.Abstractions;
using Payments.Application.CreatePayment;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Payments.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Payments.UnitTest.Application.CreatePayment;

public class CreatePaymentHandlerTests
{
    private readonly Mock<IPaymentGatewayAdapter> _paymentGateway = new();
    private readonly Mock<IPaymentRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly CreatePaymentHandler _handler;

    public CreatePaymentHandlerTests()
    {
        _handler = new CreatePaymentHandler(_paymentGateway.Object, _repository.Object, _unitOfWork.Object,
            Mock.Of<ILogger<CreatePaymentHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenAmountIsNegative()
    {
        var command = new CreatePaymentCommand(new OrderRefId(Guid.NewGuid()), -1m, Currency.Usd);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Validation);
        _repository.Verify(r => r.Add(It.IsAny<Payment>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateSucceededPayment_AndPersist_WhenGatewayApprovesCharge()
    {
        var command = new CreatePaymentCommand(new OrderRefId(Guid.NewGuid()), 42m, Currency.Usd);
        _paymentGateway.Setup(g => g.ChargeAsync(It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResult(true, null));
        Payment? added = null;
        _repository.Setup(r => r.Add(It.IsAny<Payment>())).Callback<Payment>(p => added = p);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added.Should().NotBeNull();
        added!.Status.Should().Be(PaymentStatus.Succeeded);
        added.OrderRefId.Should().Be(command.OrderRefId);
        added.Amount.Should().Be(Money.Create(Currency.Usd, 42m).Ok!);
        result.Ok.Should().Be(added.Id);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateFailedPayment_AndPersist_WhenGatewayDeclinesCharge()
    {
        var command = new CreatePaymentCommand(new OrderRefId(Guid.NewGuid()), 42m, Currency.Usd);
        _paymentGateway.Setup(g => g.ChargeAsync(It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResult(false, "Insufficient funds"));
        Payment? added = null;
        _repository.Setup(r => r.Add(It.IsAny<Payment>())).Callback<Payment>(p => added = p);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added.Should().NotBeNull();
        added!.Status.Should().Be(PaymentStatus.Failed);
        added.FailureReason.Should().Be("Insufficient funds");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
