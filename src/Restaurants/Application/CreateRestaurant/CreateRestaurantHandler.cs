using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.CreateRestaurant;

public class CreateRestaurantHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<CreateRestaurantHandler> logger)
    : IRequestHandler<CreateRestaurantCommand, Result<RestaurantId, Error>>
{
    public async Task<Result<RestaurantId, Error>> Handle(CreateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var id = new RestaurantId(Guid.NewGuid());
        var restaurant = Restaurant.Create(id, request.Name, request.Description, request.MinimalOrderPrice,
            request.Schedule);

        repository.Add(restaurant);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created restaurant {RestaurantId}", id);

        return Result<RestaurantId, Error>.Success(id);
    }
}