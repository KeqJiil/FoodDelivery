using MediatR;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.DeactivateRestaurant;

public class DeactivateRestaurantHandler(IRestaurantRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateRestaurantCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(DeactivateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.Id, cancellationToken);
        if (restaurant is null) return Result<Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));

        restaurant.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Error>.Success();
    }
}