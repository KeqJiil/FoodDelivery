using MediatR;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeMenuItemDescription;

public record ChangeMenuItemDescriptionCommand(RestaurantId RestaurantId, MenuItemId MenuItemId,
    Description NewDescription) : IRequest<Result<Error>>;
