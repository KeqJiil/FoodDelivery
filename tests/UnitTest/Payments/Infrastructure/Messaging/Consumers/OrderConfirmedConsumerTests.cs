using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using Payments.Application.CreatePayment;
using Payments.Domain.Ids;
using Payments.Infrastructure.Messaging.Consumers;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Infrastructure.IntegrationEvents;

namespace Payments.UnitTest.Infrastructure.Messaging.Consumers;

public class OrderConfirmedConsumerTests
{
    private readonly Mock<ISender> _sender = new();

    private readonly OrderConfirmedConsumer _consumer;

    public OrderConfirmedConsumerTests()
    {
        _consumer = new OrderConfirmedConsumer(_sender.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderConfirmedConsumer>>());
    }

    [Fact]
    public async Task Consume_ShouldSendCreatePaymentCommand_WithMappedOrderIdAndAmount()
    {
        var orderId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentId, Error>.Success(new PaymentId(Guid.NewGuid())));
        var context = Mock.Of<ConsumeContext<OrderConfirmedIntegration>>(c =>
            c.Message == new OrderConfirmedIntegration(orderId, 42m, Currency.Usd));

        await _consumer.Consume(context);

        _sender.Verify(s => s.Send(
            It.Is<CreatePaymentCommand>(c => c.OrderRefId == new OrderRefId(orderId)
                                              && c.Amount == 42m && c.Currency == Currency.Usd),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenCommandSucceeds()
    {
        _sender.Setup(s => s.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentId, Error>.Success(new PaymentId(Guid.NewGuid())));
        var context = Mock.Of<ConsumeContext<OrderConfirmedIntegration>>(c =>
            c.Message == new OrderConfirmedIntegration(Guid.NewGuid(), 42m, Currency.Usd));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldThrow_WhenCommandFails()
    {
        _sender.Setup(s => s.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentId, Error>.Fail(Error.NotFound("Order not found")));
        var context = Mock.Of<ConsumeContext<OrderConfirmedIntegration>>(c =>
            c.Message == new OrderConfirmedIntegration(Guid.NewGuid(), 42m, Currency.Usd));

        var act = () => _consumer.Consume(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
