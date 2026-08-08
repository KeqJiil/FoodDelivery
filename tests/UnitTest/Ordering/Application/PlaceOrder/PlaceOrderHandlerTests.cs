using FluentAssertions;
using Moq;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using Ordering.Application.Abstractions;
using Ordering.Application.PlaceOrder;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;

namespace Ordering.UnitTest.Application.PlaceOrder;

public class PlaceOrderHandlerTests
{
    private Mock<IOrderRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();
    private Mock<IRestaurantMinimumOrderPriceAdapter> _minimumPriceAdapter = new();
    private Mock<IRestaurantActiveAdapter> _activeAdapter = new();

    private readonly PlaceOrderHandler _handler;

    public PlaceOrderHandlerTests()
    {
        _handler = new PlaceOrderHandler(_repository.Object, _unitOfWork.Object, _minimumPriceAdapter.Object,
            _activeAdapter.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<PlaceOrderHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderNotFound()
    {
        var command = new PlaceOrderCommand(new OrderId(Guid.NewGuid()));
        _repository.Setup(r => r.GetByIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        var command = new PlaceOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _minimumPriceAdapter.Setup(a =>
                a.GetMinimumPriceForOrderAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
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
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        var command = new PlaceOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _minimumPriceAdapter.Setup(a =>
                a.GetMinimumPriceForOrderAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Money.Create(Currency.Usd, 15m).Ok!);
        _activeAdapter.Setup(a => a.IsActiveAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        order.Status.Should().Be(OrderStatus.Draft);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantIsNotActive()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        var command = new PlaceOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _minimumPriceAdapter.Setup(a =>
                a.GetMinimumPriceForOrderAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Money.Create(Currency.Usd, 1m).Ok!);
        _activeAdapter.Setup(a => a.IsActiveAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Validation);
        order.Status.Should().Be(OrderStatus.Draft);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPlaceOrder_AndPersist_WhenValid()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));
        var command = new PlaceOrderCommand(order.Id);
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _minimumPriceAdapter.Setup(a =>
                a.GetMinimumPriceForOrderAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Money.Create(Currency.Usd, 1m).Ok!);
        _activeAdapter.Setup(a => a.IsActiveAsync(order.RestaurantRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Pending);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}