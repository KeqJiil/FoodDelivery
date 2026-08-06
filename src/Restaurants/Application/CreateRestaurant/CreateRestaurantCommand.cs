using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.CreateRestaurant;

public record CreateRestaurantCommand(
    string Name,
    string Description,
    Currency Currency,
    decimal Amount,
    List<(DayOfWeek OpenDay, TimeOnly OpenTime, DayOfWeek CloseDay, TimeOnly CloseTime)>? Schedule)
    : IRequest<Result<RestaurantId, Error>>;
