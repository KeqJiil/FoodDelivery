using FluentAssertions;
using Restaurants.Domain.Entities;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.UnitTest.Domain.Entities;

public class MenuItemTests
{
    private static MenuItem CreateMenuItem(Currency currency = Currency.Usd, decimal price = 12m)
    {
        return MenuItem.Create(new MenuItemId(), Name.Create("Margherita").Ok!,
            Description.Create("Classic tomato and mozzarella").Ok!, Money.Create(currency, price).Ok!);
    }

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var id = new MenuItemId();
        var name = Name.Create("Margherita").Ok!;
        var description = Description.Create("Classic tomato and mozzarella").Ok!;
        var price = Money.Create(Currency.Usd, 12m).Ok!;

        var menuItem = MenuItem.Create(id, name, description, price);

        menuItem.Id.Should().Be(id);
        menuItem.Name.Should().Be(name);
        menuItem.Description.Should().Be(description);
        menuItem.Price.Should().Be(price);
    }

    [Fact]
    public void ChangeName_ShouldUpdateName()
    {
        var menuItem = CreateMenuItem();

        menuItem.ChangeName(Name.Create("Diavola").Ok!);

        menuItem.Name.Data.Should().Be("Diavola");
    }

    [Fact]
    public void ChangeDescription_ShouldUpdateDescription()
    {
        var menuItem = CreateMenuItem();
        const string newDescription = "Spicy salami and mozzarella cheese";

        menuItem.ChangeDescription(Description.Create(newDescription).Ok!);

        menuItem.Description.Data.Should().Be(newDescription);
    }

    [Fact]
    public void ChangePrice_ShouldUpdatePrice_WhenSameCurrency()
    {
        var menuItem = CreateMenuItem(Currency.Usd, 12m);

        var result = menuItem.ChangePrice(Money.Create(Currency.Usd, 15m).Ok!);

        result.IsSuccess.Should().BeTrue();
        menuItem.Price.Amount.Should().Be(15m);
    }

    [Fact]
    public void ChangePrice_ShouldFail_AndKeepOriginalPrice_WhenCurrencyMismatches()
    {
        var menuItem = CreateMenuItem(Currency.Usd, 12m);

        var result = menuItem.ChangePrice(Money.Create(Currency.Eur, 15m).Ok!);

        result.IsSuccess.Should().BeFalse();
        menuItem.Price.Amount.Should().Be(12m);
        menuItem.Price.Currency.Should().Be(Currency.Usd);
    }
}
