using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using Payments.Application.CancelPayment;
using Payments.Infrastructure.Messaging.Consumers;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace Payments.UnitTest.Infrastructure.Messaging.Consumers;

public class CancelPaymentConsumerTests
{
    private readonly Mock<ISender> _sender = new();

    private readonly CancelPaymentConsumer _consumer;

    public CancelPaymentConsumerTests()
    {
        _consumer = new CancelPaymentConsumer(
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CancelPaymentConsumer>>(), _sender.Object);
    }

    private void SetupResult(Result<Error> result)
    {
        _sender.Setup(s => s.Send(It.IsAny<CancelPaymentByOrderIdCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task Consume_ShouldSendCancelCommand_WithMappedOrderId()
    {
        var orderId = Guid.NewGuid();
        SetupResult(Result<Error>.Success());
        var context = Mock.Of<ConsumeContext<CancelPayment>>(c => c.Message == new CancelPayment(orderId));

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(It.Is<CancelPaymentByOrderIdCommand>(c => c.OrderId == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(ErrorEnum.Conflict)]
    [InlineData(ErrorEnum.NotFound)]
    public async Task Consume_ShouldSwallowError_WhenPaymentCannotBeCancelled(ErrorEnum errorType)
    {
        var error = errorType == ErrorEnum.Conflict
            ? Error.Conflict("Cannot cancel a payment that is already Succeeded")
            : Error.NotFound("Not found");
        SetupResult(Result<Error>.Fail(error));
        var context = Mock.Of<ConsumeContext<CancelPayment>>(c => c.Message == new CancelPayment(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenErrorIsUnexpected()
    {
        SetupResult(Result<Error>.Fail(Error.Unexpected()));
        var context = Mock.Of<ConsumeContext<CancelPayment>>(c => c.Message == new CancelPayment(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenErrorIsValidation()
    {
        SetupResult(Result<Error>.Fail(Error.Validation("Invalid")));
        var context = Mock.Of<ConsumeContext<CancelPayment>>(c => c.Message == new CancelPayment(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenCancellationSucceeds()
    {
        SetupResult(Result<Error>.Success());
        var context = Mock.Of<ConsumeContext<CancelPayment>>(c => c.Message == new CancelPayment(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }
}
