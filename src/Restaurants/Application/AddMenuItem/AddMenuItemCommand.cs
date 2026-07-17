using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.AddMenuItem;

public record AddMenuItemCommand(RestaurantId RestaurantId, string Name, string Description, Currency Currency,
    decimal Amount) : IRequest<Result<MenuItemId, Error>>;
