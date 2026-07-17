using FluentAssertions;
using Moq;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using Ordering.Application.Abstractions;
using Ordering.Application.ChangeOrderLinePrice;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;

namespace Ordering.UnitTest.Application.ChangeOrderLinePrice;

public class ChangeOrderLinePriceHandlerTests
{
    private Mock<IOrderRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ChangeOrderLinePriceHandler _handler;

    public ChangeOrderLinePriceHandlerTests()
    {
        _handler = new ChangeOrderLinePriceHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ChangeOrderLinePriceHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenNoOrdersFound()
    {
        var menuItemRefId = new MenuItemRefId(Guid.NewGuid());
        var command = new ChangeOrderLinePriceCommand(menuItemRefId, Currency.Usd, 5m);
        _repository.Setup(r => r.GetByMenuItemIdAsync(menuItemRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdatePriceOnAllMatchingOrders_AndPersist()
    {
        var menuItemRefId = new MenuItemRefId(Guid.NewGuid());
        var order1 = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order1.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!, menuItemRefId);
        var order2 = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order2.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!, menuItemRefId);

        var newPrice = Money.Create(Currency.Usd, 7m).Ok!;
        var command = new ChangeOrderLinePriceCommand(menuItemRefId, Currency.Usd, 7m);
        _repository.Setup(r => r.GetByMenuItemIdAsync(menuItemRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { order1, order2 });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order1.OrderLines[0].Price.Should().Be(newPrice);
        order2.OrderLines[0].Price.Should().Be(newPrice);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldContinueAndPersist_WhenOneOrderFailsToUpdate()
    {
        var menuItemRefId = new MenuItemRefId(Guid.NewGuid());

        var draftOrder = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        draftOrder.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!, menuItemRefId);

        var placedOrder = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        placedOrder.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!, menuItemRefId);
        placedOrder.Place(Money.Create(Currency.Usd, 1m).Ok!);

        var newPrice = Money.Create(Currency.Usd, 7m).Ok!;
        var command = new ChangeOrderLinePriceCommand(menuItemRefId, Currency.Usd, 7m);
        _repository.Setup(r => r.GetByMenuItemIdAsync(menuItemRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { draftOrder, placedOrder });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        draftOrder.OrderLines[0].Price.Should().Be(newPrice);
        placedOrder.OrderLines[0].Price.Amount.Should().Be(10m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}