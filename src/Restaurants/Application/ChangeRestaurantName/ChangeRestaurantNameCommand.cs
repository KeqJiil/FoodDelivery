using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeRestaurantName;

public record ChangeRestaurantNameCommand(RestaurantId Id, string NewName) : IRequest<Result<Error>>;
