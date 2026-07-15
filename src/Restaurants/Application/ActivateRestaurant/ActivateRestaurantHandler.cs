using MediatR;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ActivateRestaurant;

public class ActivateRestaurantHandler(IRestaurantRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateRestaurantCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(ActivateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.Id, cancellationToken);
        if (restaurant is null) return Result<Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));

        restaurant.Activate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Error>.Success();
    }
}