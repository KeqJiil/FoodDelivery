using FluentAssertions;
using Moq;
using Ordering.Application.Abstractions;
using Ordering.Application.CreateOrder;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;

namespace Ordering.UnitTest.Application.CreateOrder;

public class CreateOrderHandlerTests
{
    private Mock<IOrderRepository> _repository = new();
    private Mock<IUnitOfWork> _unitOfWork = new();

    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _handler = new CreateOrderHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CreateOrderHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldCreateDraftOrder_AndPersistIt()
    {
        var command = new CreateOrderCommand(new RestaurantRefId(Guid.NewGuid()));
        Order? added = null;
        _repository.Setup(r => r.Add(It.IsAny<Order>())).Callback<Order>(o => added = o);

        await _handler.Handle(command, CancellationToken.None);

        added.Should().NotBeNull();
        added!.Status.Should().Be(OrderStatus.Draft);
        added.RestaurantRefId.Should().Be(command.RestaurantRefId);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewOrderId()
    {
        var command = new CreateOrderCommand(new RestaurantRefId(Guid.NewGuid()));
        Order? added = null;
        _repository.Setup(r => r.Add(It.IsAny<Order>())).Callback<Order>(o => added = o);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Ok.Should().Be(added!.Id);
    }
}