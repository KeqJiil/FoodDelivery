using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Application.GetRestaurantById;

public record MenuItemDto(Guid Id, string Name, string Description, Money Price);
