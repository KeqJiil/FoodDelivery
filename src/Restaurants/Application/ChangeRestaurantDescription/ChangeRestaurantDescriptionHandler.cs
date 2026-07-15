using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeRestaurantDescription;

public class ChangeRestaurantDescriptionHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ChangeRestaurantDescriptionHandler> logger)
    : IRequestHandler<ChangeRestaurantDescriptionCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(ChangeRestaurantDescriptionCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.Id, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Change description failed: restaurant {RestaurantId} not found", request.Id);
            return Result<Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));
        }

        var result = restaurant.ChangeDescription(request.NewDescription);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to change description of restaurant {RestaurantId}: {Error}", request.Id,
                result.Error);
            return Result<Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Changed description of restaurant {RestaurantId}", request.Id);

        return Result<Error>.Success();
    }
}