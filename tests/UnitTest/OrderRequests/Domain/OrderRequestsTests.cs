using FluentAssertions;
using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Ids;

namespace OrderRequests.UnitTest.Domain;

public class OrderRequestsTests
{
    [Fact]
    public void Create_ShouldReturnPendingOrderRequest_WithGivenIds()
    {
        var id = new OrderRequestId(Guid.NewGuid());
        var orderRefId = new OrderRefId(Guid.NewGuid());
        var restaurantRefId = new RestaurantRefId(Guid.NewGuid());

        var orderRequest = OrderRequest.Create(id, orderRefId, restaurantRefId);

        orderRequest.Id.Should().Be(id);
        orderRequest.OrderRefId.Should().Be(orderRefId);
        orderRequest.RestaurantRefId.Should().Be(restaurantRefId);
        orderRequest.Status.Should().Be(OrderRequestStatus.Pending);
    }

    [Fact]
    public void Approve_ShouldTransitionToApproved_WhenPending()
    {
        var orderRequest = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()),
            new OrderRefId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));

        var result = orderRequest.Approve();

        result.IsSuccess.Should().BeTrue();
        orderRequest.Status.Should().Be(OrderRequestStatus.Approved);
    }

    [Fact]
    public void Approve_ShouldFail_WhenStatusIsNotPending()
    {
        var orderRequest = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()),
            new OrderRefId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        orderRequest.Approve();

        var result = orderRequest.Approve();

        result.IsSuccess.Should().BeFalse();
        orderRequest.Status.Should().Be(OrderRequestStatus.Approved);
    }

    [Fact]
    public void Reject_ShouldTransitionToRejected_WhenPending()
    {
        var orderRequest = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()),
            new OrderRefId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));

        var result = orderRequest.Reject();

        result.IsSuccess.Should().BeTrue();
        orderRequest.Status.Should().Be(OrderRequestStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldFail_WhenStatusIsNotPending()
    {
        var orderRequest = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()),
            new OrderRefId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        orderRequest.Reject();

        var result = orderRequest.Reject();

        result.IsSuccess.Should().BeFalse();
        orderRequest.Status.Should().Be(OrderRequestStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldFail_WhenAlreadyApproved()
    {
        var orderRequest = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()),
            new OrderRefId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        orderRequest.Approve();

        var result = orderRequest.Reject();

        result.IsSuccess.Should().BeFalse();
        orderRequest.Status.Should().Be(OrderRequestStatus.Approved);
    }
}
