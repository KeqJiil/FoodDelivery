using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Domain.Policies;

public class OrderCanBePlacedPolicy
{
    public static Result<Error> CanBePlaced(Order order, Money minimalPrice, bool isRestaurantActive = false)
    {
        if (!OrderStatusChangePolicy.CanChangeStatusTo(order.Status, OrderStatus.Pending))
            return Result<Error>.Fail(Error.Conflict("Status can't be changed"));

        if (!isRestaurantActive) return Result<Error>.Fail(Error.Validation("Restaurant is not active"));

        if (order.OrderLines.Count == 0) return Result<Error>.Fail(Error.Validation("No order lines"));

        return minimalPrice.CompareTo(order.TotalPrice) <= 0
            ? Result<Error>.Success()
            : Result<Error>.Fail(Error.Validation("Order price is too small"));
    }
}