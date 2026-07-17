using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeRestaurantDescription;

public record ChangeRestaurantDescriptionCommand(RestaurantId Id, string NewDescription) : IRequest<Result<Error>>;
