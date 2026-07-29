using Deliveries.Application.CreateDelivery;
using Deliveries.Domain.Ids;
using Deliveries.Infrastructure.Messaging.Consumers;
using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace Deliveries.UnitTest.Infrastructure.Messaging.Consumers;

public class CreateDeliveryConsumerTests
{
    private readonly Mock<ISender> _sender = new();

    private readonly CreateDeliveryConsumer _consumer;

    public CreateDeliveryConsumerTests()
    {
        _consumer = new CreateDeliveryConsumer(_sender.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CreateDeliveryConsumer>>());
    }

    [Fact]
    public async Task Consume_ShouldSendCreateDeliveryCommand_WithMappedOrderId()
    {
        var orderId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<CreateDeliveryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DeliveryId, Error>.Success(new DeliveryId(Guid.NewGuid())));
        var context = Mock.Of<ConsumeContext<CreateDelivery>>(c =>
            c.Message == new CreateDelivery(orderId));

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(
            It.Is<CreateDeliveryCommand>(c => c.OrderRefId == new OrderRefId(orderId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenCommandSucceeds()
    {
        _sender.Setup(s => s.Send(It.IsAny<CreateDeliveryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DeliveryId, Error>.Success(new DeliveryId(Guid.NewGuid())));
        var context = Mock.Of<ConsumeContext<CreateDelivery>>(c =>
            c.Message == new CreateDelivery(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenCommandFails()
    {
        _sender.Setup(s => s.Send(It.IsAny<CreateDeliveryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DeliveryId, Error>.Fail(Error.Unexpected()));
        var context = Mock.Of<ConsumeContext<CreateDelivery>>(c =>
            c.Message == new CreateDelivery(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenCommandFailsWithConflict()
    {
        _sender.Setup(s => s.Send(It.IsAny<CreateDeliveryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DeliveryId, Error>.Fail(Error.Conflict("Delivery already exists")));
        var context = Mock.Of<ConsumeContext<CreateDelivery>>(c =>
            c.Message == new CreateDelivery(Guid.NewGuid()));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }
}
