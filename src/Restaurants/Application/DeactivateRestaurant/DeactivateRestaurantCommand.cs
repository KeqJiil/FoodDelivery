using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.DeactivateRestaurant;

public record DeactivateRestaurantCommand(RestaurantId Id) : IRequest<Result<Error>>;
