using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.AddMenuItem;

public class AddMenuItemHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<AddMenuItemHandler> logger)
    : IRequestHandler<AddMenuItemCommand, Result<MenuItemId, Error>>
{
    public async Task<Result<MenuItemId, Error>> Handle(AddMenuItemCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.RestaurantId, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Add menu item failed: restaurant {RestaurantId} not found", request.RestaurantId);
            return Result<MenuItemId, Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));
        }

        var menuId = new MenuItemId(Guid.NewGuid());
        var result = restaurant.AddMenuItem(menuId, request.Name, request.Description, request.Price);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to add menu item to restaurant {RestaurantId}: {Error}", request.RestaurantId,
                result.Error);
            return Result<MenuItemId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Added menu item {MenuItemId} to restaurant {RestaurantId}", menuId,
            request.RestaurantId);

        return Result<MenuItemId, Error>.Success(menuId);
    }
}