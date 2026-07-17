using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeMenuItemDescription;

public record ChangeMenuItemDescriptionCommand(RestaurantId RestaurantId, MenuItemId MenuItemId,
    string NewDescription) : IRequest<Result<Error>>;
