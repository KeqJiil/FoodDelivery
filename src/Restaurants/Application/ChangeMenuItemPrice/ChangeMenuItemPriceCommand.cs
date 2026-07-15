using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.ChangeMenuItemPrice;

public record ChangeMenuItemPriceCommand(RestaurantId RestaurantId, MenuItemId MenuItemId, Money NewPrice)
    : IRequest<Result<Error>>;
