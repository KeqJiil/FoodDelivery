using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.SetMinimalOrderPrice;

public class SetMinimalOrderPriceHandler(
    IRestaurantRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<SetMinimalOrderPriceHandler> logger)
    : IRequestHandler<SetMinimalOrderPriceCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(SetMinimalOrderPriceCommand request, CancellationToken cancellationToken)
    {
        var moneyResult = Money.Create(request.Currency, request.Amount);
        if (!moneyResult.IsSuccess) return Result<Error>.Fail(moneyResult.Error!);

        var restaurant = await repository.GetById(request.Id, cancellationToken);
        if (restaurant is null)
        {
            logger.LogWarning("Set minimal order price failed: restaurant {RestaurantId} not found", request.Id);
            return Result<Error>.Fail(Error.NotFound("Restaurant not found"));
        }

        var result = restaurant.SetMinimalOrderPrice(moneyResult.Ok!);
        if (!result.IsSuccess)
        {
            logger.LogWarning("Failed to set minimal order price of restaurant {RestaurantId}: {Error}", request.Id,
                result.Error);
            return Result<Error>.Fail(result.Error ?? Error.Unexpected());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Set minimal order price of restaurant {RestaurantId}", request.Id);

        return Result<Error>.Success();
    }
}