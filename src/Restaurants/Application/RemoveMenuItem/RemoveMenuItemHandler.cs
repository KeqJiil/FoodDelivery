using MediatR;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.RemoveMenuItem;

public class RemoveMenuItemHandler(IRestaurantRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveMenuItemCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(RemoveMenuItemCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.RestaurantId, cancellationToken);
        if (restaurant is null) return Result<Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));

        var result = restaurant.RemoveMenuItem(request.MenuItemId);
        if (!result.IsSuccess)
            return Result<Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected Error"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Error>.Success();
    }
}