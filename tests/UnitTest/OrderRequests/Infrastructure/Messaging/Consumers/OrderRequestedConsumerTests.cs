using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using OrderRequests.Application.CreateOrder;
using OrderRequests.Domain.Ids;
using OrderRequests.Infrastructure.Messaging.Consumers;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Infrastructure.IntegrationEvents;

namespace OrderRequests.UnitTest.Infrastructure.Messaging.Consumers;

public class OrderRequestedConsumerTests
{
    private Mock<ISender> _sender = new();

    private readonly OrderRequestedConsumer _consumer;

    public OrderRequestedConsumerTests()
    {
        _consumer = new OrderRequestedConsumer(_sender.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderRequestedConsumer>>());
    }

    [Fact]
    public async Task Consume_ShouldSendCreateOrderCommand_WithMappedIds()
    {
        var orderId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderRequestId, Error>.Success(new OrderRequestId(Guid.NewGuid())));
        var context = Mock.Of<ConsumeContext<OrderPlacedIntegration>>(c =>
            c.Message == new OrderPlacedIntegration(orderId, restaurantId));

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(
            It.Is<CreateOrderCommand>(c => c.OrderRefId == new OrderRefId(orderId)
                                            && c.RestaurantRefId == new RestaurantRefId(restaurantId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenCommandSucceeds()
    {
        _sender.Setup(s => s.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderRequestId, Error>.Success(new OrderRequestId(Guid.NewGuid())));
        var context = Mock.Of<ConsumeContext<OrderPlacedIntegration>>(c =>
            c.Message == new OrderPlacedIntegration(Guid.NewGuid(), Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenCommandFails()
    {
        _sender.Setup(s => s.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderRequestId, Error>.Fail(Error.Conflict("Duplicate order request")));
        var context = Mock.Of<ConsumeContext<OrderPlacedIntegration>>(c =>
            c.Message == new OrderPlacedIntegration(Guid.NewGuid(), Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
