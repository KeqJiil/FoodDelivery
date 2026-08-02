using FluentAssertions;
using Moq;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.CancelOrder;
using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Events;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain.Enums;

namespace OrderRequests.UnitTest.Application;

public class CancelOrderHandlerTests
{
    private readonly Mock<IOrderRequestRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _handler = new CancelOrderHandler(_repository.Object, _unitOfWork.Object);
    }

    private static OrderRequest PendingRequest(Guid? orderId = null) =>
        OrderRequest.Create(new OrderRequestId(Guid.NewGuid()),
            new OrderRefId(orderId ?? Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderRequestDoesNotExist()
    {
        _repository.Setup(r => r.GetByOrderRefIdAsync(It.IsAny<OrderRefId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderRequest?)null);

        var result = await _handler.Handle(new CancelOrderCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldWrapRawGuid_IntoOrderRefId()
    {
        var orderId = Guid.NewGuid();
        _repository.Setup(r => r.GetByOrderRefIdAsync(It.IsAny<OrderRefId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PendingRequest(orderId));

        await _handler.Handle(new CancelOrderCommand(orderId), CancellationToken.None);

        _repository.Verify(r => r.GetByOrderRefIdAsync(new OrderRefId(orderId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancel_AndPersist_WhenPending()
    {
        var request = PendingRequest();
        _repository.Setup(r => r.GetByOrderRefIdAsync(request.OrderRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _handler.Handle(new CancelOrderCommand(request.OrderRefId.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(OrderRequestStatus.Cancelled);
        request.Events.Should().ContainSingle(e => e is OrderCancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancel_WhenRequestWasAlreadyApproved()
    {
        var request = PendingRequest();
        request.Approve();
        _repository.Setup(r => r.GetByOrderRefIdAsync(request.OrderRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _handler.Handle(new CancelOrderCommand(request.OrderRefId.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(OrderRequestStatus.Cancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenRequestWasRejected()
    {
        var request = PendingRequest();
        request.Reject();
        _repository.Setup(r => r.GetByOrderRefIdAsync(request.OrderRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _handler.Handle(new CancelOrderCommand(request.OrderRefId.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        request.Status.Should().Be(OrderRequestStatus.Rejected);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenRequestAlreadyCancelled()
    {
        var request = PendingRequest();
        request.Cancel();
        _repository.Setup(r => r.GetByOrderRefIdAsync(request.OrderRefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var result = await _handler.Handle(new CancelOrderCommand(request.OrderRefId.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        request.Events.Should().ContainSingle(e => e is OrderCancelled);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPropagateCancellationToken_ToRepositoryAndUnitOfWork()
    {
        var request = PendingRequest();
        using var cts = new CancellationTokenSource();
        _repository.Setup(r => r.GetByOrderRefIdAsync(request.OrderRefId, cts.Token)).ReturnsAsync(request);

        await _handler.Handle(new CancelOrderCommand(request.OrderRefId.Id), cts.Token);

        _repository.Verify(r => r.GetByOrderRefIdAsync(request.OrderRefId, cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }
}
