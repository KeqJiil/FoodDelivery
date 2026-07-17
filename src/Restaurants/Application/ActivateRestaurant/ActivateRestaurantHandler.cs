using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ActivateRestaurant;

public class ActivateRestaurantHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ActivateRestaurantHandler> logger)
    : IRequestHandler<ActivateRestaurantCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(ActivateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.Id, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Activate failed: restaurant {RestaurantId} not found", request.Id);
            return Result<Error>.Fail(Error.NotFound("Restaurant not found"));
        }

        restaurant.Activate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Activated restaurant {RestaurantId}", request.Id);

        return Result<Error>.Success();
    }
}