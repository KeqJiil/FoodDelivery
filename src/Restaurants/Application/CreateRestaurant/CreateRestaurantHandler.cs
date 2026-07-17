using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

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
        var nameResult = Name.Create(request.Name);
        var descriptionResult = Description.Create(request.Description);
        var moneyResult = Money.Create(request.Currency, request.Amount);

        var checkResult = Result.Check(nameResult, descriptionResult, moneyResult);
        if (!checkResult.IsSuccess) return Result<RestaurantId, Error>.Fail(checkResult.Error!);

        var id = new RestaurantId();
        var restaurant = Restaurant.Create(id, nameResult.Ok!, descriptionResult.Ok!, moneyResult.Ok!,
            new Schedule(request.Schedule));

        repository.Add(restaurant);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created restaurant {RestaurantId}", id);

        return Result<RestaurantId, Error>.Success(id);
    }
}