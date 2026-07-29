using FluentAssertions;
using Moq;
using Ordering.Application.Abstractions;
using Ordering.Application.OrderFail;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Events;
using Ordering.Domain.Ids;
using Ordering.UnitTest.TestHelpers;
using SharedKernel.Domain.Enums;

namespace Ordering.UnitTest.Application.OrderFail;

public class OrderFailHandlerTests
{
    private readonly Mock<IOrderRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly OrderFailHandler _handler;

    public OrderFailHandlerTests()
    {
        _handler = new OrderFailHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<OrderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(new OrderFailCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldLookUpOrder_ByIdFromCommand()
    {
        var orderId = Guid.NewGuid();
        var order = OrderFactory.Pending(new OrderId(orderId));
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await _handler.Handle(new OrderFailCommand(orderId), CancellationToken.None);

        _repository.Verify(r => r.GetByIdAsync(new OrderId(orderId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFailOrder_AndPersist_WhenPending()
    {
        var order = OrderFactory.Pending();
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new OrderFailCommand(order.Id.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Failed);
        order.Events.Should().ContainSingle(e => e is OrderFailed);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFailOrder_AndPersist_WhenProcessing()
    {
        var order = OrderFactory.Processing();
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new OrderFailCommand(order.Id.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Failed);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFailWithConflict_WhenOrderIsStillDraft()
    {
        var order = OrderFactory.Draft();
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new OrderFailCommand(order.Id.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        order.Status.Should().Be(OrderStatus.Draft);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFailWithConflict_WhenOrderAlreadyFailed()
    {
        var order = OrderFactory.Failed();
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new OrderFailCommand(order.Id.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        order.Events.Should().ContainSingle(e => e is OrderFailed);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFailWithConflict_WhenOrderAlreadyCancelled()
    {
        var order = OrderFactory.Cancelled();
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new OrderFailCommand(order.Id.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        order.Status.Should().Be(OrderStatus.Cancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFailWithConflict_WhenOrderAlreadyConfirmed()
    {
        var order = OrderFactory.Confirmed();
        _repository.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new OrderFailCommand(order.Id.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        order.Status.Should().Be(OrderStatus.Confirmed);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPropagateCancellationToken_ToRepositoryAndUnitOfWork()
    {
        var order = OrderFactory.Pending();
        using var cts = new CancellationTokenSource();
        _repository.Setup(r => r.GetByIdAsync(order.Id, cts.Token)).ReturnsAsync(order);

        await _handler.Handle(new OrderFailCommand(order.Id.Id), cts.Token);

        _repository.Verify(r => r.GetByIdAsync(order.Id, cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }
}
