using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.SetMinimalOrderPrice;

public record SetMinimalOrderPriceCommand(RestaurantId Id, Currency Currency, decimal Amount)
    : IRequest<Result<Error>>;
