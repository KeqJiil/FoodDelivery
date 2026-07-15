using Restaurants.Domain.Enums;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.GetRestaurantById;

public record RestaurantDto(
    Guid Id,
    string Name,
    string Description,
    Money MinimalOrderPrice,
    RestaurantStatus Status,
    IReadOnlyList<OpeningWindow> OpeningWindows,
    IReadOnlyList<MenuItemDto> MenuItems);
