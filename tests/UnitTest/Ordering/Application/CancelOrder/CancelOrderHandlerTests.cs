using FluentAssertions;
using Moq;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using Ordering.Application.Abstractions;
using Ordering.Application.CancelOrder;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;

namespace Ordering.UnitTest.Application.CancelOrder;

public class CancelOrderHandlerTests
{
    private Mock<IOrderRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();

    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _handler = new CancelOrderHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CancelOrderHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderNotFound()
    {
        var command = new CancelOrderCommand(new OrderId(Guid.NewGuid()));
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
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        order.Place(Money.Create(Currency.Usd, 1m).Ok!);
        order.Confirm();
        var command = new CancelOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        order.Status.Should().Be(OrderStatus.Confirmed);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCancelOrder_AndPersist_WhenValid()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var command = new CancelOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}