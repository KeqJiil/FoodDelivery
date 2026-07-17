using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.UnitTest.TestHelpers;

internal static class RestaurantFactory {
    public static Restaurant CreateValid(RestaurantId? id = null, Currency currency = Currency.Usd, decimal minimalOrderPrice = 10m)
    {
        return Restaurant.Create(
            id ?? new RestaurantId(),
            Name.Create("Pizzeria").Ok!,
            Description.Create("Best pizza in town").Ok!,
            Money.Create(currency, minimalOrderPrice).Ok!);
    }
}
