using MediatR;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeRestaurantSchedule;

public record ChangeRestaurantScheduleCommand(RestaurantId Id, Schedule NewSchedule) : IRequest<Result<Error>>;
