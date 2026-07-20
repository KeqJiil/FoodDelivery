using FluentAssertions;
using Moq;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.RejectOrder;
using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain.Enums;

namespace OrderRequests.UnitTest.Application;

public class RejectOrderHandlerTests
{
    private Mock<IOrderRequestRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();

    private readonly RejectOrderHandler _handler;

    public RejectOrderHandlerTests()
    {
        _handler = new RejectOrderHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldRejectOrderRequest_AndPersistChanges()
    {
        var orderRequest = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()),
            new OrderRefId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        var command = new RejectOrderCommand(orderRequest.Id);
        _repository.Setup(r => r.GetByIdAsync(orderRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderRequest);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        orderRequest.Status.Should().Be(OrderRequestStatus.Rejected);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderRequestDoesNotExist()
    {
        var command = new RejectOrderCommand(new OrderRequestId(Guid.NewGuid()));
        _repository.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderRequest?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_AndNotPersist_WhenStatusIsNotPending()
    {
        var orderRequest = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()),
            new OrderRefId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        orderRequest.Approve();
        var command = new RejectOrderCommand(orderRequest.Id);
        _repository.Setup(r => r.GetByIdAsync(orderRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderRequest);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        orderRequest.Status.Should().Be(OrderRequestStatus.Approved);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
