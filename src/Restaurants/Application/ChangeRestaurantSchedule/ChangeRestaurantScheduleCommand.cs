using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeRestaurantSchedule;

public record ChangeRestaurantScheduleCommand(
    RestaurantId Id,
    List<(DayOfWeek OpenDay, TimeOnly OpenTime, DayOfWeek CloseDay, TimeOnly CloseTime)> NewSchedule)
    : IRequest<Result<Error>>;
