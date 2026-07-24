using Deliveries.Application.Abstractions;
using Deliveries.Application.CreateDelivery;
using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Enums;
using Deliveries.Domain.Ids;
using FluentAssertions;
using Moq;

namespace Deliveries.UnitTest.Application.CreateDelivery;

public class CreateDeliveryHandlerTests
{
    private readonly Mock<IDeliveryRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly CreateDeliveryHandler _handler;

    public CreateDeliveryHandlerTests()
    {
        _handler = new CreateDeliveryHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreatePendingDelivery_AndPersistIt()
    {
        var command = new CreateDeliveryCommand(new OrderRefId(Guid.NewGuid()));
        Delivery? added = null;
        _repository.Setup(r => r.Add(It.IsAny<Delivery>())).Callback<Delivery>(d => added = d);

        var result = await _handler.Handle(command, CancellationToken.None);

        added.Should().NotBeNull();
        added!.Status.Should().Be(DeliveryStatus.Pending);
        added.OrderRefId.Should().Be(command.OrderRefId);
        result.IsSuccess.Should().BeTrue();
        result.Ok.Should().Be(added.Id);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
