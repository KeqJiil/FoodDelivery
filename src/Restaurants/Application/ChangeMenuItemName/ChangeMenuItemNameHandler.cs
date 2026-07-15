using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeMenuItemName;

public class ChangeMenuItemNameHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ChangeMenuItemNameHandler> logger)
    : IRequestHandler<ChangeMenuItemNameCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(ChangeMenuItemNameCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.RestaurantId, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Change menu item name failed: restaurant {RestaurantId} not found",
                request.RestaurantId);
            return Result<Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));
        }

        var result = restaurant.ChangeMenuItemName(request.MenuItemId, request.NewName);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to change name of menu item {MenuItemId} in restaurant {RestaurantId}: {Error}",
                request.MenuItemId, request.RestaurantId, result.Error);
            return Result<Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Changed name of menu item {MenuItemId} in restaurant {RestaurantId}",
            request.MenuItemId, request.RestaurantId);

        return Result<Error>.Success();
    }
}