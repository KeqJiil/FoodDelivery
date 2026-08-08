using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Enums;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using Restaurants.Infrastructure.Persistence;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace FoodDelivery.IntegrationTest.Persistence.Restaurants;

public class RestaurantPersistenceTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SaveAndReload_ShouldPersistRestaurantWithScheduleAndMenu()
    {
        var schedule = new Schedule([
            new OpeningWindow(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0))
        ]);

        var restaurant = Restaurant.Create(new RestaurantId(Guid.NewGuid()), Name.Create("Sushi House").Ok!,
            Description.Create("Best sushi rolls in town, freshly made every day").Ok!,
            Money.Create(Currency.Usd, 15m).Ok!, schedule);

        restaurant.AddMenuItem(new MenuItemId(Guid.NewGuid()), Name.Create("California Roll").Ok!,
            Description.Create("Crab, avocado and cucumber wrapped in rice and nori").Ok!,
            Money.Create(Currency.Usd, 8m).Ok!);

        await using (var writeContext = CreateRestaurantsContext())
        {
            writeContext.Restaurants.Add(restaurant);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateRestaurantsContext();
        var reloaded = await readContext.Restaurants
            .Include(r => r.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == restaurant.Id);

        reloaded.Should().NotBeNull();
        reloaded!.Status.Should().Be(RestaurantStatus.Active);
        reloaded.Name.Should().Be(Name.Create("Sushi House").Ok);
        reloaded.MinimalOrderPrice.Should().Be(Money.Create(Currency.Usd, 15m).Ok!);
        reloaded.Schedule.OpeningWindows.Should().ContainSingle()
            .Which.Should()
            .Be(new OpeningWindow(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0)));
        reloaded.MenuItems.Should().ContainSingle().Which.Price.Should().Be(Money.Create(Currency.Usd, 8m).Ok!);
    }

    private RestaurantsDbContext CreateRestaurantsContext()
    {
        return CreateContext<RestaurantsDbContext>("restaurants", o => new RestaurantsDbContext(o));
    }
}