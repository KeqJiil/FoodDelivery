using FluentAssertions;
using Moq;
using Ordering.Application.Abstractions;
using Ordering.Application.ConfirmOrder;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.UnitTest.Application.ConfirmOrder;

public class ConfirmOrderHandlerTests
{
    private Mock<IOrderRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ConfirmOrderHandler _handler;

    public ConfirmOrderHandlerTests()
    {
        _handler = new ConfirmOrderHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderNotFound()
    {
        var command = new ConfirmOrderCommand(new OrderId(Guid.NewGuid()));
        _repository.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenStatusCannotChange()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()));
        var command = new ConfirmOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        order.Status.Should().Be(OrderStatus.Draft);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldConfirmOrder_AndPersist_WhenValid()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()));
        order.Place(new Money(Currency.Usd, 1m));
        var command = new ConfirmOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
