using Restaurants.Domain.Ids;
using SharedKernel.Domain;

namespace Restaurants.Domain.Events;

public class MenuItemPriceChanged(RestaurantId id) : DomainEvent<RestaurantId>(id);