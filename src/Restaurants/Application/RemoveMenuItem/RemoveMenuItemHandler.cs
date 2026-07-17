using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.RemoveMenuItem;

public class RemoveMenuItemHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<RemoveMenuItemHandler> logger)
    : IRequestHandler<RemoveMenuItemCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(RemoveMenuItemCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.RestaurantId, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Remove menu item failed: restaurant {RestaurantId} not found", request.RestaurantId);
            return Result<Error>.Fail(Error.NotFound("Restaurant not found"));
        }

        var result = restaurant.RemoveMenuItem(request.MenuItemId);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to remove menu item {MenuItemId} from restaurant {RestaurantId}: {Error}",
                request.MenuItemId, request.RestaurantId, result.Error);
            return Result<Error>.Fail(result.Error ?? Error.Unexpected());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Removed menu item {MenuItemId} from restaurant {RestaurantId}", request.MenuItemId,
            request.RestaurantId);

        return Result<Error>.Success();
    }
}