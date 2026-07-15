using MediatR;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeRestaurantName;

public record ChangeRestaurantNameCommand(RestaurantId Id, Name NewName) : IRequest<Result<Error>>;
