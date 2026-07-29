using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using Ordering.Application.OrderFail;
using Ordering.Infrastructure.Messaging.Consumers;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace Ordering.UnitTest.Infrastructure.Messaging.Consumers;

public class OrderFailConsumerTests
{
    private readonly Mock<ISender> _sender = new();

    private readonly OrderFailConsumer _consumer;

    public OrderFailConsumerTests()
    {
        _consumer = new OrderFailConsumer(_sender.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderFailConsumer>>());
    }

    private void SetupResult(Result<Error> result)
    {
        _sender.Setup(s => s.Send(It.IsAny<OrderFailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task Consume_ShouldSendOrderFailCommand_WithMappedOrderId()
    {
        var orderId = Guid.NewGuid();
        SetupResult(Result<Error>.Success());
        var context = Mock.Of<ConsumeContext<OrderFail>>(c => c.Message == new OrderFail(orderId));

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(It.Is<OrderFailCommand>(c => c.OrderId == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldPropagateCancellationToken_FromConsumeContext()
    {
        using var cts = new CancellationTokenSource();
        SetupResult(Result<Error>.Success());
        var context = Mock.Of<ConsumeContext<OrderFail>>(c =>
            c.Message == new OrderFail(Guid.NewGuid()) && c.CancellationToken == cts.Token);

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(It.IsAny<OrderFailCommand>(), cts.Token), Times.Once);
    }

    [Theory]
    [InlineData(ErrorEnum.Conflict)]
    [InlineData(ErrorEnum.NotFound)]
    public async Task Consume_ShouldSwallowError_WhenOrderAlreadyInTerminalState(ErrorEnum errorType)
    {
        var error = errorType == ErrorEnum.Conflict
            ? Error.Conflict("Status can't be changed")
            : Error.NotFound("Not found");
        SetupResult(Result<Error>.Fail(error));
        var context = Mock.Of<ConsumeContext<OrderFail>>(c => c.Message == new OrderFail(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenErrorIsUnexpected()
    {
        var orderId = Guid.NewGuid();
        SetupResult(Result<Error>.Fail(Error.Unexpected()));
        var context = Mock.Of<ConsumeContext<OrderFail>>(c => c.Message == new OrderFail(orderId));

        var act = () => _consumer.Consume(context);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain(orderId.ToString());
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenErrorIsValidation()
    {
        SetupResult(Result<Error>.Fail(Error.Validation("Invalid")));
        var context = Mock.Of<ConsumeContext<OrderFail>>(c => c.Message == new OrderFail(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenCommandSucceeds()
    {
        SetupResult(Result<Error>.Success());
        var context = Mock.Of<ConsumeContext<OrderFail>>(c => c.Message == new OrderFail(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }
}
