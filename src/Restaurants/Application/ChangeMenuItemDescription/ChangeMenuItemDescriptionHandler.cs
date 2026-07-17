using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeMenuItemDescription;

public class ChangeMenuItemDescriptionHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ChangeMenuItemDescriptionHandler> logger)
    : IRequestHandler<ChangeMenuItemDescriptionCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(ChangeMenuItemDescriptionCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.RestaurantId, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Change menu item description failed: restaurant {RestaurantId} not found",
                request.RestaurantId);
            return Result<Error>.Fail(Error.NotFound("Restaurant not found"));
        }

        var result = restaurant.ChangeMenuItemDescription(request.MenuItemId, request.NewDescription);
        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Failed to change description of menu item {MenuItemId} in restaurant {RestaurantId}: {Error}",
                request.MenuItemId, request.RestaurantId, result.Error);
            return Result<Error>.Fail(result.Error ?? Error.Unexpected());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Changed description of menu item {MenuItemId} in restaurant {RestaurantId}",
            request.MenuItemId, request.RestaurantId);

        return Result<Error>.Success();
    }
}