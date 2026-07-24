using Deliveries.Application.Abstractions;
using Deliveries.Application.CompleteDelivery;
using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Enums;
using Deliveries.Domain.Ids;
using FluentAssertions;
using Moq;
using SharedKernel.Domain.Enums;

namespace Deliveries.UnitTest.Application.CompleteDelivery;

public class CompleteDeliveryHandlerTests
{
    private readonly Mock<IDeliveryRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly CompleteDeliveryHandler _handler;

    public CompleteDeliveryHandlerTests()
    {
        _handler = new CompleteDeliveryHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ShouldCompleteDelivery_AndPersistChanges()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));
        delivery.MarkPickedUp();
        var command = new CompleteDeliveryCommand(delivery.Id);
        _repository.Setup(r => r.GetByIdAsync(delivery.Id, It.IsAny<CancellationToken>())).ReturnsAsync(delivery);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        delivery.Status.Should().Be(DeliveryStatus.Delivered);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenNotPickedUpYet()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));
        var command = new CompleteDeliveryCommand(delivery.Id);
        _repository.Setup(r => r.GetByIdAsync(delivery.Id, It.IsAny<CancellationToken>())).ReturnsAsync(delivery);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.Conflict);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenDeliveryDoesNotExist()
    {
        var command = new CompleteDeliveryCommand(new DeliveryId(Guid.NewGuid()));
        _repository.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorEnum.NotFound);
    }
}
