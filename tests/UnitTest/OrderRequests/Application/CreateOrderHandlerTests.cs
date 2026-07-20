using FluentAssertions;
using Moq;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.CreateOrder;
using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Ids;

namespace OrderRequests.UnitTest.Application;

public class CreateOrderHandlerTests
{
    private Mock<IOrderRequestRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();

    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _handler = new CreateOrderHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreatePendingOrderRequest_AndPersistIt()
    {
        var command = new CreateOrderCommand(new OrderRefId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        OrderRequest? added = null;
        _repository.Setup(r => r.Add(It.IsAny<OrderRequest>())).Callback<OrderRequest>(o => added = o);

        await _handler.Handle(command, CancellationToken.None);

        added.Should().NotBeNull();
        added!.Status.Should().Be(OrderRequestStatus.Pending);
        added.OrderRefId.Should().Be(command.OrderRefId);
        added.RestaurantRefId.Should().Be(command.RestaurantRefId);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewOrderRequestId()
    {
        var command = new CreateOrderCommand(new OrderRefId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        OrderRequest? added = null;
        _repository.Setup(r => r.Add(It.IsAny<OrderRequest>())).Callback<OrderRequest>(o => added = o);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Ok.Should().Be(added!.Id);
    }
}
