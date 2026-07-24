using Deliveries.Application.Abstractions;
using Deliveries.Application.FailDelivery;
using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Enums;
using Deliveries.Domain.Ids;
using FluentAssertions;
using Moq;
using SharedKernel.Domain.Enums;

namespace Deliveries.UnitTest.Application.FailDelivery;

public class FailDeliveryHandlerTests
{
    private readonly Mock<IDeliveryRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly FailDeliveryHandler _handler;

    public FailDeliveryHandlerTests()
    {
        _handler = new FailDeliveryHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldFailDelivery_AndPersistChanges()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));
        var command = new FailDeliveryCommand(delivery.Id, "Address not found");
        _repository.Setup(r => r.GetByIdAsync(delivery.Id, It.IsAny<CancellationToken>())).ReturnsAsync(delivery);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        delivery.Status.Should().Be(DeliveryStatus.Failed);
        delivery.FailureReason.Should().Be("Address not found");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenDeliveryDoesNotExist()
    {
        var command = new FailDeliveryCommand(new DeliveryId(Guid.NewGuid()), "Address not found");
        _repository.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
    }
}
