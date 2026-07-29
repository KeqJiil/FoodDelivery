using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using OrderRequests.Application.CancelOrder;
using OrderRequests.Infrastructure.Messaging.Consumers;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace OrderRequests.UnitTest.Infrastructure.Messaging.Consumers;

public class OrderCancelConsumerTests
{
    private readonly Mock<ISender> _sender = new();

    private readonly OrderCancelConsumer _consumer;

    public OrderCancelConsumerTests()
    {
        _consumer = new OrderCancelConsumer(_sender.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderCancelConsumer>>());
    }

    private void SetupResult(Result<Error> result)
    {
        _sender.Setup(s => s.Send(It.IsAny<CancelOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task Consume_ShouldSendCancelOrderCommand_WithMappedOrderId()
    {
        var orderId = Guid.NewGuid();
        SetupResult(Result<Error>.Success());
        var context = Mock.Of<ConsumeContext<CancelOrderRequest>>(c =>
            c.Message == new CancelOrderRequest(orderId));

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(It.Is<CancelOrderCommand>(c => c.Id == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldPropagateCancellationToken_FromConsumeContext()
    {
        using var cts = new CancellationTokenSource();
        SetupResult(Result<Error>.Success());
        var context = Mock.Of<ConsumeContext<CancelOrderRequest>>(c =>
            c.Message == new CancelOrderRequest(Guid.NewGuid()) && c.CancellationToken == cts.Token);

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(It.IsAny<CancelOrderCommand>(), cts.Token), Times.Once);
    }

    [Theory]
    [InlineData(ErrorEnum.Conflict)]
    [InlineData(ErrorEnum.NotFound)]
    public async Task Consume_ShouldSwallowError_WhenRequestCannotBeCancelled(ErrorEnum errorType)
    {
        var error = errorType == ErrorEnum.Conflict
            ? Error.Conflict("Cannot cancel an order request that is already Rejected")
            : Error.NotFound("Not found");
        SetupResult(Result<Error>.Fail(error));
        var context = Mock.Of<ConsumeContext<CancelOrderRequest>>(c =>
            c.Message == new CancelOrderRequest(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenErrorIsUnexpected()
    {
        SetupResult(Result<Error>.Fail(Error.Unexpected()));
        var context = Mock.Of<ConsumeContext<CancelOrderRequest>>(c =>
            c.Message == new CancelOrderRequest(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenErrorIsValidation()
    {
        SetupResult(Result<Error>.Fail(Error.Validation("Invalid")));
        var context = Mock.Of<ConsumeContext<CancelOrderRequest>>(c =>
            c.Message == new CancelOrderRequest(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenCancellationSucceeds()
    {
        SetupResult(Result<Error>.Success());
        var context = Mock.Of<ConsumeContext<CancelOrderRequest>>(c =>
            c.Message == new CancelOrderRequest(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }
}
