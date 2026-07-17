using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.ChangeMenuItemPrice;

public class ChangeMenuItemPriceHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ChangeMenuItemPriceHandler> logger)
    : IRequestHandler<ChangeMenuItemPriceCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(ChangeMenuItemPriceCommand request, CancellationToken cancellationToken)
    {
        var moneyResult = Money.Create(request.Currency, request.Amount);
        if (!moneyResult.IsSuccess) return Result<Error>.Fail(moneyResult.Error!);

        var restaurant = await repository.GetById(request.RestaurantId, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Change menu item price failed: restaurant {RestaurantId} not found",
                request.RestaurantId);
            return Result<Error>.Fail(Error.NotFound("Restaurant not found"));
        }

        var result = restaurant.ChangeMenuItemPrice(request.MenuItemId, moneyResult.Ok!);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to change price of menu item {MenuItemId} in restaurant {RestaurantId}: {Error}",
                request.MenuItemId, request.RestaurantId, result.Error);
            return Result<Error>.Fail(result.Error ?? Error.Unexpected());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Changed price of menu item {MenuItemId} in restaurant {RestaurantId}",
            request.MenuItemId, request.RestaurantId);

        return Result<Error>.Success();
    }
}