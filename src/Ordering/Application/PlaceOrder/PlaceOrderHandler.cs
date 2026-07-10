using MediatR;
using Ordering.Application.Abstractions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.PlaceOrder;

public class PlaceOrderHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork,
    IRestaurantMinimumOrderPriceAdapter restaurantMinimumOrderPriceAdapter)
    : IRequestHandler<PlaceOrderCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null) return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Order not found"));

        var minimumPrice =
            await restaurantMinimumOrderPriceAdapter.GetMinimumPriceForOrderAsync(order.RestaurantRefId,
                cancellationToken);
        if (minimumPrice is null)
            return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));

        var result = order.Place(minimumPrice);
        if (!result.IsSuccess)
            return Result<OrderId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderId, Error>.Success(order.Id);
    }
}