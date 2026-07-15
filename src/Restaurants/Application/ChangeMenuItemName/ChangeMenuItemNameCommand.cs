using MediatR;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeMenuItemName;

public record ChangeMenuItemNameCommand(RestaurantId RestaurantId, MenuItemId MenuItemId, Name NewName)
    : IRequest<Result<Error>>;
