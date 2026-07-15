using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.SetMinimalOrderPrice;

public record SetMinimalOrderPriceCommand(RestaurantId Id, Money Price) : IRequest<Result<Error>>;
