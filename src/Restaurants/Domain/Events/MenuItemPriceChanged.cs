using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Domain.Events;

public record MenuItemPriceChanged(RestaurantId Id, MenuItemId MenuId, Money NewPrice) : DomainEvent<RestaurantId>(Id);