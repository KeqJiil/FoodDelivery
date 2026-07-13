using FluentAssertions;
using Moq;
using Ordering.Application.Abstractions;
using Ordering.Application.RemoveOrderLineItem;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.UnitTest.Application.RemoveOrderLineItem;

public class RemoveOrderLineItemHandlerTests
{
    private Mock<IOrderRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();

    private readonly RemoveOrderLineItemHandler _handler;

    public RemoveOrderLineItemHandlerTests()
    {
        _handler = new RemoveOrderLineItemHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderNotFound()
    {
        var command = new RemoveOrderLineItemCommand(new OrderId(Guid.NewGuid()), new OrderLineId(Guid.NewGuid()));
        _repository.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Order not found");
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderRejectsRemoval()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var orderLineId = new OrderLineId(Guid.NewGuid());
        order.AddOrderLineItem(orderLineId, new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()));
        order.Place(new Money(Currency.Usd, 1m));

        var command = new RemoveOrderLineItemCommand(order.Id, orderLineId);
        _repository.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRemoveLine_AndPersist_WhenValid()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var orderLineId = new OrderLineId(Guid.NewGuid());
        order.AddOrderLineItem(orderLineId, new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()));

        var command = new RemoveOrderLineItemCommand(order.Id, orderLineId);
        _repository.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.OrderLines.Should().BeEmpty();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}