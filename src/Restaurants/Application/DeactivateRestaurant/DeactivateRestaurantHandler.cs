using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.DeactivateRestaurant;

public class DeactivateRestaurantHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<DeactivateRestaurantHandler> logger)
    : IRequestHandler<DeactivateRestaurantCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(DeactivateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.Id, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Deactivate failed: restaurant {RestaurantId} not found", request.Id);
            return Result<Error>.Fail(Error.NotFound("Restaurant not found"));
        }

        restaurant.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deactivated restaurant {RestaurantId}", request.Id);

        return Result<Error>.Success();
    }
}