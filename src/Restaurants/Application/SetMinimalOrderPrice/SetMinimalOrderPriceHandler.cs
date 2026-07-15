using MediatR;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.SetMinimalOrderPrice;

public class SetMinimalOrderPriceHandler(IRestaurantRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<SetMinimalOrderPriceCommand, Result<Error>>
{
    public async Task<Result<Error>> Handle(SetMinimalOrderPriceCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await repository.GetById(request.Id, cancellationToken);
        if (restaurant is null) return Result<Error>.Fail(new Error(ErrorEnum.NotFound, "Restaurant not found"));

        var result = restaurant.SetMinimalOrderPrice(request.Price);
        if (!result.IsSuccess)
            return Result<Error>.Fail(result.Error ?? new Error(ErrorEnum.Unexpected, "Unexpected error"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Error>.Success();
    }
}