using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeMenuItemName;

public record ChangeMenuItemNameCommand(RestaurantId RestaurantId, MenuItemId MenuItemId, string NewName)
    : IRequest<Result<Error>>;
