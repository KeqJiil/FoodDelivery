using MediatR;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Application.RemoveMenuItem;

public record RemoveMenuItemCommand(RestaurantId RestaurantId, MenuItemId MenuItemId) : IRequest<Result<Error>>;
