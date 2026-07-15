using MediatR;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.AddMenuItem;

public record AddMenuItemCommand(RestaurantId RestaurantId, Name Name, Description Description, Money Price)
    : IRequest<Result<MenuItemId, Error>>;
