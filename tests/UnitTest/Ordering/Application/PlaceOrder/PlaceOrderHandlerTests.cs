using FluentAssertions;
using Moq;
using Ordering.Application.Abstractions;
using Ordering.Application.PlaceOrder;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.UnitTest.Application.PlaceOrder;

public class PlaceOrderHandlerTests
{
    private Mock<IOrderRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();
    private Mock<IRestaurantMinimumOrderPriceAdapter> _minimumPriceAdapter = new();

    private readonly PlaceOrderHandler _handler;

    public PlaceOrderHandlerTests()
    {
        _handler = new PlaceOrderHandler(_repository.Object, _unitOfWork.Object, _minimumPriceAdapter.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderNotFound()
    {
        var command = new PlaceOrderCommand(new OrderId(Guid.NewGuid()));
        _repository.Setup(r => r.GetByIdAsync(command.OrderId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()));
        var command = new PlaceOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _minimumPriceAdapter.Setup(a => a.GetMinimumPriceForOrderAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Money?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderCannotBePlaced()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()));
        var command = new PlaceOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _minimumPriceAdapter.Setup(a => a.GetMinimumPriceForOrderAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Money(Currency.Usd, 15m));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        order.Status.Should().Be(OrderStatus.Draft);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPlaceOrder_AndPersist_WhenValid()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), new Money(Currency.Usd, 10m), new MenuItemRefId(Guid.NewGuid()));
        var command = new PlaceOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _minimumPriceAdapter.Setup(a => a.GetMinimumPriceForOrderAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Money(Currency.Usd, 1m));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Pending);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
