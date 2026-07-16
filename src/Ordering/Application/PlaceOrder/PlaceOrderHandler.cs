using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Domain.Aggregates;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;

namespace Ordering.Application.PlaceOrder;

public class PlaceOrderHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork,
    IRestaurantMinimumOrderPriceAdapter restaurantMinimumOrderPriceAdapter,
    ILogger<PlaceOrderHandler> logger)
    : IRequestHandler<PlaceOrderCommand, Result<OrderId, Error>>
{
    public async Task<Result<OrderId, Error>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Place order failed: order {OrderId} not found", request.OrderId);
            return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Order not found"));
        }

        var minimumPrice =
            await restaurantMinimumOrderPriceAdapter.GetMinimumPriceForOrderAsync(order.RestaurantRefId,
                cancellationToken);
        if (minimumPrice is null)
        {
            logger.LogWarning("Place order failed: restaurant {RestaurantRefId} not found", order.RestaurantRefId);
            return Result<OrderId, Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));
        }

        var result = order.Place(minimumPrice);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to place order {OrderId}: {Error}", order.Id, result.Error);
            return Result<OrderId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Placed order {OrderId}", order.Id);

        return Result<OrderId, Error>.Success(order.Id);
    }
}