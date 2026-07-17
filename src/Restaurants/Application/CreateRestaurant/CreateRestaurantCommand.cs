using MediatR;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.CreateRestaurant;

public record CreateRestaurantCommand(
    string Name,
    string Description,
    Currency Currency,
    decimal Amount,
    List<OpeningWindow>? Schedule) : IRequest<Result<RestaurantId, Error>>;
