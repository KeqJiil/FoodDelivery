using FluentAssertions;
using Ordering.Domain.Enums;
using Ordering.Domain.Policies;

namespace Ordering.UnitTest.Domain.Policies;

public class OrderStatusChangePolicyTests
{
    [Theory]
    [InlineData(OrderStatus.Draft, OrderStatus.Pending)]
    [InlineData(OrderStatus.Draft, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed)]
    public void CanChangeStatusTo_ShouldReturnTrue_ForAllowedTransitions(OrderStatus current, OrderStatus next)
    {
        var result = OrderStatusChangePolicy.CanChangeStatusTo(current, next);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(OrderStatus.Draft, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed)]
    public void CanChangeStatusTo_ShouldReturnFalse_ForDisallowedTransitions(OrderStatus current, OrderStatus next)
    {
        var result = OrderStatusChangePolicy.CanChangeStatusTo(current, next);
        result.Should().BeFalse();
    }
}