using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Domain.Policies;

public class OrderCanBePlacedPolicy
{
    public static Result<Error> CanBePlaced(Order order, Money minimalPrice)
    {
        if (!OrderStatusChangePolicy.CanChangeStatusTo(order.Status, OrderStatus.Pending))
            return Result<Error>.Fail(new Error(ErrorEnum.Conflict, "Status can't be changed"));

        if (order.OrderLines.Count == 0) return Result<Error>.Fail(new Error(ErrorEnum.Validation, "No order lines"));
        
        return minimalPrice.CompareTo(order.TotalPrice) <= 0 
            ? Result<Error>.Success()
            : Result<Error>.Fail(new Error(ErrorEnum.Validation, "Order price is too small"));
    }
}