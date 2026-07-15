using MediatR;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeMenuItemName;

public class ChangeMenuItemNameHandler(IRestaurantRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeMenuItemNameCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(ChangeMenuItemNameCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.RestaurantId, cancellationToken);
        if (restaurant is null) return Result<Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));

        var result = restaurant.ChangeMenuItemName(request.MenuItemId, request.NewName);
        if (!result.IsSuccess)
            return Result<Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Error>.Success();
    }
}