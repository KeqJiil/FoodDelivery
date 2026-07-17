using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.ChangeMenuItemPrice;

public record ChangeMenuItemPriceCommand(RestaurantId RestaurantId, MenuItemId MenuItemId, Currency Currency,
    decimal Amount) : IRequest<Result<Error>>;
