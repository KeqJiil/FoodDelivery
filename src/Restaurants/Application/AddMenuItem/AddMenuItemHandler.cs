using MediatR;
using Restaurants.Application.Abstractions;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.AddMenuItem;

public class AddMenuItemHandler(IRestaurantRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<AddMenuItemCommand, Result<MenuItemId, Error>>
{
    public async Task<Result<MenuItemId, Error>> Handle(AddMenuItemCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.RestaurantId, cancellationToken);
        if (restaurant is null)
            return Result<MenuItemId, Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));

        var menuId = new MenuItemId(Guid.NewGuid());
        var result = restaurant.AddMenuItem(menuId, request.Name, request.Description, request.Price);
        if (!result.IsSuccess)
            return Result<MenuItemId, Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MenuItemId, Error>.Success(menuId);
    }
}