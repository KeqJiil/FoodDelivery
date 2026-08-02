using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using Restaurants.Infrastructure.Persistence;
using Restaurants.Infrastructure.Persistence.Readers;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace FoodDelivery.IntegrationTest.Persistence.Restaurants;

public class RestaurantReaderTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnRestaurantWithScheduleAndMenu()
    {
        var schedule = new Schedule([
            new OpeningWindow(DayOfWeek.Friday, new TimeOnly(10, 0), DayOfWeek.Friday, new TimeOnly(23, 0))
        ]);

        var restaurant = Restaurant.Create(new RestaurantId(Guid.NewGuid()), Name.Create("Pizza Place").Ok!,
            Description.Create("Wood-fired pizza with fresh local ingredients").Ok!,
            Money.Create(Currency.Usd, 12m).Ok!, schedule);

        restaurant.AddMenuItem(new MenuItemId(Guid.NewGuid()), Name.Create("Margherita").Ok!,
            Description.Create("Tomato, mozzarella and fresh basil leaves").Ok!,
            Money.Create(Currency.Usd, 9m).Ok!);

        await using (var writeContext = CreateRestaurantsContext())
        {
            writeContext.Restaurants.Add(restaurant);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateRestaurantsContext();
        var reader = new RestaurantReader(readContext);

        var dto = await reader.GetByIdAsync(restaurant.Id.Id);

        dto.Should().NotBeNull();
        dto!.Name.Should().Be("Pizza Place");
        dto.Description.Should().Be("Wood-fired pizza with fresh local ingredients");
        dto.MinimalOrderPrice.Should().Be(Money.Create(Currency.Usd, 12m).Ok);
        dto.OpeningWindows.Should().ContainSingle();
        dto.MenuItems.Should().ContainSingle().Which.Name.Should().Be("Margherita");
    }

    [Fact]
    public async Task GetMenuItemPriceByIdAsync_ShouldReturnPrice_WhenItemExists()
    {
        var restaurant = Restaurant.Create(new RestaurantId(Guid.NewGuid()), Name.Create("Burger Spot").Ok!,
            Description.Create("Classic American burgers and crispy fries").Ok!,
            Money.Create(Currency.Usd, 10m).Ok!);

        var menuItemId = new MenuItemId(Guid.NewGuid());
        restaurant.AddMenuItem(menuItemId, Name.Create("Cheeseburger").Ok!,
            Description.Create("Beef patty with cheddar cheese and pickles").Ok!,
            Money.Create(Currency.Usd, 7.5m).Ok!);

        await using (var writeContext = CreateRestaurantsContext())
        {
            writeContext.Restaurants.Add(restaurant);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateRestaurantsContext();
        var reader = new RestaurantReader(readContext);

        var price = await reader.GetMenuItemPriceByIdAsync(menuItemId.Id);

        price.Should().Be(Money.Create(Currency.Usd, 7.5m).Ok);
    }

    [Fact]
    public async Task GetMenuItemPriceByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        await using var readContext = CreateRestaurantsContext();
        var reader = new RestaurantReader(readContext);

        var price = await reader.GetMenuItemPriceByIdAsync(Guid.NewGuid());

        price.Should().BeNull();
    }

    private RestaurantsDbContext CreateRestaurantsContext()
    {
        return CreateContext<RestaurantsDbContext>("restaurants", o => new RestaurantsDbContext(o));
    }
}