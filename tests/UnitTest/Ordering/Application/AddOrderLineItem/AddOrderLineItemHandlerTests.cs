using FluentAssertions;
using Moq;
using Ordering.Application.AddOrderLineItem;
using Ordering.Application.Abstractions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.UnitTest.Application.AddOrderLineItem;

public class AddOrderLineItemHandlerTests
{
    private Mock<IOrderRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();
    private Mock<IMenuPriceForOrderLineAdapter> _menuPriceAdapter = new();

    private readonly AddOrderLineItemHandler _handler;

    public AddOrderLineItemHandlerTests()
    {
        _handler = new AddOrderLineItemHandler(_repository.Object, _unitOfWork.Object, _menuPriceAdapter.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderNotFound()
    {
        var command = new AddOrderLineItemCommand(new OrderId(Guid.NewGuid()), new MenuItemRefId(Guid.NewGuid()));
        _repository.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMenuItemPriceNotFound()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var menuItemRefId = new MenuItemRefId(Guid.NewGuid());
        var command = new AddOrderLineItemCommand(order.Id, menuItemRefId);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _menuPriceAdapter.Setup(a => a.GetMenuItemPriceByIdAsync(menuItemRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Money?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderRejectsLine()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m),
            new MenuItemRefId(Guid.NewGuid()));
        order.Place(new Money(Currency.Usd, 1m));
        var menuItemRefId = new MenuItemRefId(Guid.NewGuid());
        var command = new AddOrderLineItemCommand(order.Id, menuItemRefId);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _menuPriceAdapter.Setup(a => a.GetMenuItemPriceByIdAsync(menuItemRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Money(Currency.Usd, 5m));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAddNewLine_AndPersist_WhenValid()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var menuItemRefId = new MenuItemRefId(Guid.NewGuid());
        var command = new AddOrderLineItemCommand(order.Id, menuItemRefId);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _menuPriceAdapter.Setup(a => a.GetMenuItemPriceByIdAsync(menuItemRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Money(Currency.Usd, 5m));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.OrderLines.Should().ContainSingle(l => l.MenuItemRefId == menuItemRefId);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}