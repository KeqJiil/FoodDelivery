using MediatR;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.CreateRestaurant;

public record CreateRestaurantCommand(Name Name, Description Description, Money MinimalOrderPrice, Schedule? Schedule)
    : IRequest<Result<RestaurantId, Error>>;
