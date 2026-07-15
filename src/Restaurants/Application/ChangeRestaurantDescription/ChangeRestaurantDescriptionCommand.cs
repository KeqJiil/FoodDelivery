using MediatR;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeRestaurantDescription;

public record ChangeRestaurantDescriptionCommand(RestaurantId Id, Description NewDescription)
    : IRequest<Result<Error>>;
