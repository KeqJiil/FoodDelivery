using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Enums;
using Deliveries.Domain.Ids;
using FluentAssertions;

namespace Deliveries.UnitTest.Domain.Aggregates;

public class DeliveryTests
{
    [Fact]
    public void Create_ShouldReturnPendingDelivery_WithGivenIds()
    {
        var id = new DeliveryId(Guid.NewGuid());
        var orderRefId = new OrderRefId(Guid.NewGuid());

        var delivery = Delivery.Create(id, orderRefId);

        delivery.Id.Should().Be(id);
        delivery.OrderRefId.Should().Be(orderRefId);
        delivery.Status.Should().Be(DeliveryStatus.Pending);
    }

    [Fact]
    public void MarkPickedUp_ShouldTransitionToPickedUp_WhenPending()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));

        var result = delivery.MarkPickedUp();

        result.IsSuccess.Should().BeTrue();
        delivery.Status.Should().Be(DeliveryStatus.PickedUp);
    }

    [Fact]
    public void MarkPickedUp_ShouldFail_WhenStatusIsNotPending()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));
        delivery.MarkPickedUp();

        var result = delivery.MarkPickedUp();

        result.IsSuccess.Should().BeFalse();
        delivery.Status.Should().Be(DeliveryStatus.PickedUp);
    }

    [Fact]
    public void Complete_ShouldTransitionToDelivered_WhenPickedUp()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));
        delivery.MarkPickedUp();

        var result = delivery.Complete();

        result.IsSuccess.Should().BeTrue();
        delivery.Status.Should().Be(DeliveryStatus.Delivered);
    }

    [Fact]
    public void Complete_ShouldFail_WhenStatusIsNotPickedUp()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));

        var result = delivery.Complete();

        result.IsSuccess.Should().BeFalse();
        delivery.Status.Should().Be(DeliveryStatus.Pending);
    }

    [Fact]
    public void Fail_ShouldTransitionToFailed_AndStoreReason_WhenPending()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));

        var result = delivery.Fail("Address not found");

        result.IsSuccess.Should().BeTrue();
        delivery.Status.Should().Be(DeliveryStatus.Failed);
        delivery.FailureReason.Should().Be("Address not found");
    }

    [Fact]
    public void Fail_ShouldTransitionToFailed_WhenPickedUp()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));
        delivery.MarkPickedUp();

        var result = delivery.Fail("Recipient unreachable");

        result.IsSuccess.Should().BeTrue();
        delivery.Status.Should().Be(DeliveryStatus.Failed);
    }

    [Fact]
    public void Fail_ShouldFail_WhenAlreadyDelivered()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));
        delivery.MarkPickedUp();
        delivery.Complete();

        var result = delivery.Fail("Too late");

        result.IsSuccess.Should().BeFalse();
        delivery.Status.Should().Be(DeliveryStatus.Delivered);
    }
}
