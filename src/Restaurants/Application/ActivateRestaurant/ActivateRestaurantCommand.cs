using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ActivateRestaurant;

public record ActivateRestaurantCommand(RestaurantId Id) : IRequest<Result<Error>>;
