using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using Ordering.Application.CancelOrder;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Messaging.Consumers;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace Ordering.UnitTest.Infrastructure.Messaging.Consumers;

public class CancelOrderConsumerTests
{
    private Mock<ISender> _sender = new();

    private readonly CancelOrderConsumer _consumer;

    public CancelOrderConsumerTests()
    {
        _consumer = new CancelOrderConsumer(_sender.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CancelOrderConsumer>>());
    }

    [Fact]
    public async Task Consume_ShouldSendCancelOrderCommand_WithMappedOrderId()
    {
        var orderId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<CancelOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderId, Error>.Success(new OrderId(orderId)));
        var context =
            Mock.Of<ConsumeContext<CancelOrder>>(c =>
                c.Message == new CancelOrder(orderId));

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(
            It.Is<CancelOrderCommand>(c => c.Id == new OrderId(orderId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenResultFailsWithConflictOrNotFound()
    {
        var orderId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<CancelOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderId, Error>.Fail(Error.Conflict("Some conflict")));

        var context =
            Mock.Of<ConsumeContext<CancelOrder>>(c =>
                c.Message == new CancelOrder(orderId));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenResultFailsWithUnexpectedError()
    {
        var orderId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<CancelOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderId, Error>.Fail(Error.Unexpected()));

        var context =
            Mock.Of<ConsumeContext<CancelOrder>>(c =>
                c.Message == new CancelOrder(orderId));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}