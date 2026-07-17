using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using Ordering.Application.ConfirmOrder;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Messaging.Consumers;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace Ordering.UnitTest.Infrastructure.Messaging.Consumers;

public class ConfirmOrderConsumerTests
{
    private Mock<ISender> _sender = new();

    private readonly ConfirmOrderConsumer _consumer;

    public ConfirmOrderConsumerTests()
    {
        _consumer = new ConfirmOrderConsumer(_sender.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ConfirmOrderConsumer>>());
    }

    [Fact]
    public async Task Consume_ShouldSendConfirmOrderCommand_WithMappedOrderId()
    {
        var orderId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<ConfirmOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderId, Error>.Success(new OrderId(orderId)));
        var context = Mock.Of<ConsumeContext<ConfirmOrder>>(c => c.Message == new ConfirmOrder(orderId));

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(
            It.Is<ConfirmOrderCommand>(c => c.Id == new OrderId(orderId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenResultFailsWithConflictOrNotFound()
    {
        var orderId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<ConfirmOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderId, Error>.Fail(Error.Conflict("Some conflict")));

        var context = Mock.Of<ConsumeContext<ConfirmOrder>>(c => c.Message == new ConfirmOrder(orderId));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenResultFailsWithUnexpectedError()
    {
        var orderId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<ConfirmOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderId, Error>.Fail(Error.Unexpected()));

        var context = Mock.Of<ConsumeContext<ConfirmOrder>>(c => c.Message == new ConfirmOrder(orderId));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}