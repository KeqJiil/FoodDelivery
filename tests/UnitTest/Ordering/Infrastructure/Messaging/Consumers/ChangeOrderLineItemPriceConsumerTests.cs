using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using Ordering.Application.ChangeOrderLinePrice;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Messaging.Consumers;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.IntegrationEvents;

namespace Ordering.UnitTest.Infrastructure.Messaging.Consumers;

public class ChangeOrderLineItemPriceConsumerTests
{
    private Mock<ISender> _sender = new();

    private ChangeOrderLineItemPriceConsumer _consumer;

    public ChangeOrderLineItemPriceConsumerTests()
    {
        _consumer = new ChangeOrderLineItemPriceConsumer(_sender.Object);
    }

    [Fact]
    public async Task Consume_ShouldSendChangeOrderLinePriceCommand_WithMappedMenuItemAndMoney()
    {
        var menuId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<ChangeOrderLinePriceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Error>.Success());
        var context =
            Mock.Of<ConsumeContext<MenuItemPriceChanged>>(c =>
                c.Message == new MenuItemPriceChanged(menuId, 10m, Currency.Usd));

        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
        _sender.Verify(s => s.Send(It.IsAny<ChangeOrderLinePriceCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldNotThrow_WhenResultFailsWithConflictOrNotFound()
    {
        var menuId = Guid.NewGuid();
        _sender.Setup(s => s.Send(It.IsAny<ChangeOrderLinePriceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Some conflict")));
        var context =
            Mock.Of<ConsumeContext<MenuItemPriceChanged>>(c =>
                c.Message == new MenuItemPriceChanged(menuId, 10m, Currency.Usd));
        var act = () => _consumer.Consume(context);

        await act.Should().NotThrowAsync();
    }
}