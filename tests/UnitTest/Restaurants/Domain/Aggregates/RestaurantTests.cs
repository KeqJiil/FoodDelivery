using FluentAssertions;
using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Enums;
using Restaurants.Domain.Events;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.UnitTest.Domain.Aggregates;

public class RestaurantTests
{
    private static Restaurant CreateRestaurant(Currency currency = Currency.Usd, decimal minimalOrderPrice = 10m)
    {
        return Restaurant.Create(new RestaurantId(), Name.Create("Pizzeria").Ok!,
            Description.Create("Best pizza in town").Ok!, Money.Create(currency, minimalOrderPrice).Ok!);
    }

    [Fact]
    public void Create_ShouldReturnActiveRestaurant_WithNoMenuItems()
    {
        var restaurant = CreateRestaurant();

        restaurant.Status.Should().Be(RestaurantStatus.Active);
        restaurant.MenuItems.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldUseEmptySchedule_WhenNoneProvided()
    {
        var restaurant = CreateRestaurant();

        restaurant.Schedule.OpeningWindows.Should().BeEmpty();
    }

    [Fact]
    public void ChangeName_ShouldUpdateName()
    {
        var restaurant = CreateRestaurant();

        var result = restaurant.ChangeName(Name.Create("New Name").Ok!);

        result.IsSuccess.Should().BeTrue();
        restaurant.Name.Data.Should().Be("New Name");
    }

    [Fact]
    public void ChangeDescription_ShouldUpdateDescription()
    {
        var restaurant = CreateRestaurant();
        const string newDescription = "A brand new description of this place";

        var result = restaurant.ChangeDescription(Description.Create(newDescription).Ok!);

        result.IsSuccess.Should().BeTrue();
        restaurant.Description.Data.Should().Be(newDescription);
    }

    [Fact]
    public void ChangeSchedule_ShouldUpdateSchedule()
    {
        var restaurant = CreateRestaurant();
        var schedule = new Schedule([new OpeningWindow(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0))]);

        var result = restaurant.ChangeSchedule(schedule);

        result.IsSuccess.Should().BeTrue();
        restaurant.Schedule.Should().Be(schedule);
    }

    [Fact]
    public void Activate_ShouldSetStatusToActive()
    {
        var restaurant = CreateRestaurant();
        restaurant.Deactivate();

        restaurant.Activate();

        restaurant.Status.Should().Be(RestaurantStatus.Active);
        restaurant.IsActive().Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetStatusToInactive()
    {
        var restaurant = CreateRestaurant();

        restaurant.Deactivate();

        restaurant.Status.Should().Be(RestaurantStatus.Inactive);
        restaurant.IsActive().Should().BeFalse();
    }

    [Fact]
    public void IsOpen_ShouldBeFalse_WhenRestaurantIsInactive_EvenDuringOpeningWindow()
    {
        var schedule = new Schedule([new OpeningWindow(DayOfWeek.Monday, new TimeOnly(0, 0), DayOfWeek.Sunday, new TimeOnly(23, 59))]);
        var restaurant = Restaurant.Create(new RestaurantId(), Name.Create("Pizzeria").Ok!,
            Description.Create("Best pizza in town").Ok!, Money.Create(Currency.Usd, 10m).Ok!, schedule);
        restaurant.Deactivate();

        restaurant.IsOpen(DateTimeOffset.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsOpen_ShouldBeTrue_WhenActiveAndWithinScheduleWindow()
    {
        var monday = new DateTimeOffset(2026, 7, 20, 12, 0, 0, TimeSpan.Zero);
        var schedule = new Schedule([new OpeningWindow(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0))]);
        var restaurant = Restaurant.Create(new RestaurantId(), Name.Create("Pizzeria").Ok!,
            Description.Create("Best pizza in town").Ok!, Money.Create(Currency.Usd, 10m).Ok!, schedule);

        restaurant.IsOpen(monday).Should().BeTrue();
    }

    [Fact]
    public void SetMinimalOrderPrice_ShouldUpdatePrice_WhenSameCurrency()
    {
        var restaurant = CreateRestaurant(Currency.Usd);

        var result = restaurant.SetMinimalOrderPrice(Money.Create(Currency.Usd, 25m).Ok!);

        result.IsSuccess.Should().BeTrue();
        restaurant.MinimalOrderPrice.Amount.Should().Be(25m);
    }

    [Fact]
    public void SetMinimalOrderPrice_ShouldFail_WhenCurrencyMismatches()
    {
        var restaurant = CreateRestaurant(Currency.Usd);

        var result = restaurant.SetMinimalOrderPrice(Money.Create(Currency.Eur, 25m).Ok!);

        result.IsSuccess.Should().BeFalse();
        restaurant.MinimalOrderPrice.Currency.Should().Be(Currency.Usd);
    }

    [Fact]
    public void AddMenuItem_ShouldAddItem_WhenSameCurrency()
    {
        var restaurant = CreateRestaurant(Currency.Usd);
        var menuItemId = new MenuItemId();

        var result = restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!,
            Description.Create("Classic tomato and mozzarella").Ok!, Money.Create(Currency.Usd, 12m).Ok!);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Should().ContainSingle(m => m.Id == menuItemId);
    }

    [Fact]
    public void AddMenuItem_ShouldFail_WhenCurrencyMismatches()
    {
        var restaurant = CreateRestaurant(Currency.Usd);

        var result = restaurant.AddMenuItem(new MenuItemId(), Name.Create("Margherita").Ok!,
            Description.Create("Classic tomato and mozzarella").Ok!, Money.Create(Currency.Eur, 12m).Ok!);

        result.IsSuccess.Should().BeFalse();
        restaurant.MenuItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMenuItem_ShouldRemoveItem_WhenPresent()
    {
        var restaurant = CreateRestaurant(Currency.Usd);
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!,
            Description.Create("Classic tomato and mozzarella").Ok!, Money.Create(Currency.Usd, 12m).Ok!);

        var result = restaurant.RemoveMenuItem(menuItemId);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMenuItem_ShouldFail_WhenNotFound()
    {
        var restaurant = CreateRestaurant(Currency.Usd);

        var result = restaurant.RemoveMenuItem(new MenuItemId());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ChangeMenuItemName_ShouldUpdateName_WhenFound()
    {
        var restaurant = CreateRestaurant(Currency.Usd);
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!,
            Description.Create("Classic tomato and mozzarella").Ok!, Money.Create(Currency.Usd, 12m).Ok!);

        var result = restaurant.ChangeMenuItemName(menuItemId, Name.Create("Diavola").Ok!);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Single().Name.Data.Should().Be("Diavola");
    }

    [Fact]
    public void ChangeMenuItemName_ShouldFail_WhenNotFound()
    {
        var restaurant = CreateRestaurant(Currency.Usd);

        var result = restaurant.ChangeMenuItemName(new MenuItemId(), Name.Create("Diavola").Ok!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ChangeMenuItemDescription_ShouldUpdateDescription_WhenFound()
    {
        var restaurant = CreateRestaurant(Currency.Usd);
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!,
            Description.Create("Classic tomato and mozzarella").Ok!, Money.Create(Currency.Usd, 12m).Ok!);
        const string newDescription = "Spicy salami and mozzarella cheese";

        var result = restaurant.ChangeMenuItemDescription(menuItemId, Description.Create(newDescription).Ok!);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Single().Description.Data.Should().Be(newDescription);
    }

    [Fact]
    public void ChangeMenuItemDescription_ShouldFail_WhenNotFound()
    {
        var restaurant = CreateRestaurant(Currency.Usd);

        var result = restaurant.ChangeMenuItemDescription(new MenuItemId(),
            Description.Create("Spicy salami and mozzarella cheese").Ok!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ChangeMenuItemPrice_ShouldUpdatePrice_AndRaiseMenuItemPriceChanged_WhenSameCurrency()
    {
        var restaurant = CreateRestaurant(Currency.Usd);
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!,
            Description.Create("Classic tomato and mozzarella").Ok!, Money.Create(Currency.Usd, 12m).Ok!);

        var result = restaurant.ChangeMenuItemPrice(menuItemId, Money.Create(Currency.Usd, 15m).Ok!);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Single().Price.Amount.Should().Be(15m);
        restaurant.Events.Should().ContainSingle(e => e is MenuItemPriceChanged);
    }

    [Fact]
    public void ChangeMenuItemPrice_ShouldFail_AndNotRaiseEvent_WhenCurrencyMismatches()
    {
        var restaurant = CreateRestaurant(Currency.Usd);
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!,
            Description.Create("Classic tomato and mozzarella").Ok!, Money.Create(Currency.Usd, 12m).Ok!);

        var result = restaurant.ChangeMenuItemPrice(menuItemId, Money.Create(Currency.Eur, 15m).Ok!);

        result.IsSuccess.Should().BeFalse();
        restaurant.MenuItems.Single().Price.Amount.Should().Be(12m);
        restaurant.Events.Should().BeEmpty();
    }

    [Fact]
    public void ChangeMenuItemPrice_ShouldFail_WhenMenuItemNotFound()
    {
        var restaurant = CreateRestaurant(Currency.Usd);

        var result = restaurant.ChangeMenuItemPrice(new MenuItemId(), Money.Create(Currency.Usd, 15m).Ok!);

        result.IsSuccess.Should().BeFalse();
        restaurant.Events.Should().BeEmpty();
    }
}
