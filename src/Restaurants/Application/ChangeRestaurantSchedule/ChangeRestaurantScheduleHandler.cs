using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeRestaurantSchedule;

public class ChangeRestaurantScheduleHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ChangeRestaurantScheduleHandler> logger)
    : IRequestHandler<ChangeRestaurantScheduleCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(ChangeRestaurantScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.Id, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Change schedule failed: restaurant {RestaurantId} not found", request.Id);
            return Result<Error>.Fail(Error.NotFound("Restaurant not found"));
        }

        var result = restaurant.ChangeSchedule(new Schedule(request.NewSchedule));
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to change schedule of restaurant {RestaurantId}: {Error}", request.Id,
                result.Error);
            return Result<Error>.Fail(result.Error ?? Error.Unexpected());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Changed schedule of restaurant {RestaurantId}", request.Id);

        return Result<Error>.Success();
    }
}