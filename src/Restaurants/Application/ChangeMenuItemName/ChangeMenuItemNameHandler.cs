using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
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
        var nameResult = Name.Create(request.NewName);
        if (!nameResult.IsSuccess) return Result<Error>.Fail(nameResult.Error!);

        var restaurant = await repository.GetById(request.RestaurantId, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Change menu item name failed: restaurant {RestaurantId} not found",
                request.RestaurantId);
            return Result<Error>.Fail(Error.NotFound("Restaurant not found"));
        }

        var result = restaurant.ChangeMenuItemName(request.MenuItemId, nameResult.Ok!);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to change name of menu item {MenuItemId} in restaurant {RestaurantId}: {Error}",
                request.MenuItemId, request.RestaurantId, result.Error);
            return Result<Error>.Fail(result.Error ?? Error.Unexpected());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Changed name of menu item {MenuItemId} in restaurant {RestaurantId}",
            request.MenuItemId, request.RestaurantId);

        return Result<Error>.Success();
    }
}