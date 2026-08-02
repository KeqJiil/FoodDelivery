using System.Net;
using System.Net.Http.Json;
using Api.Controllers.Restaurants;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Restaurants.Domain.Enums;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain.Enums;

namespace FoodDelivery.IntegrationTest.Api;

[Collection("Api")]
public class RestaurantsEndpointsTests
{
    private readonly HttpClient _client;

    public RestaurantsEndpointsTests(FoodDeliveryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateRestaurant_HappyScenario()
    {
        var req = new CreateRestaurantRequest("Restaurant", "Restaurant", 100, Currency.Usd,
            new List<OpeningWindow>
                { new(DayOfWeek.Friday, new TimeOnly(12, 12), DayOfWeek.Monday, new TimeOnly(13, 13)) });

        var result = await _client.PostAsJsonAsync("v1/restaurants", req);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateRestaurant_FailScenario()
    {
        var req = new CreateRestaurantRequest("R", "r", -1, Currency.Usd,
            new List<OpeningWindow>
                { new(DayOfWeek.Friday, new TimeOnly(12, 12), DayOfWeek.Monday, new TimeOnly(13, 13)) });

        var result = await _client.PostAsJsonAsync("v1/restaurants", req);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRestaurantById_FailScenario()
    {
        var result = await _client.GetAsync($"v1/restaurants/{Guid.NewGuid()}");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRestaurantById_HappyScenario()
    {
        var id = await TestData.SeedRestaurant(_client);

        var result = await _client.GetAsync($"v1/restaurants/{id}");

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangeRestaurantName_FailScenario_NoSuchRestaurant()
    {
        var result =
            await _client.PatchAsJsonAsync($"v1/restaurants/{Guid.NewGuid()}/name", new { Name = "New restaurant" });

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeRestaurantName_FailScenario_WrongName()
    {
        var id = await TestData.SeedRestaurant(_client);

        var result = await _client.PatchAsJsonAsync($"v1/restaurants/{id}/name", new { Name = "1" });

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var response = await _client.GetAsync($"v1/restaurants/{id}");
        var restaurant = await response.Content.ReadFromJsonAsync<RestaurantTestDto>(TestData.JsonOptions);

        restaurant!.Name.Should().Be("restaurant");
    }

    [Fact]
    public async Task ChangeRestaurantName_FailScenario_HappyScenario()
    {
        var id = await TestData.SeedRestaurant(_client);

        var result = await _client.PatchAsJsonAsync($"v1/restaurants/{id}/name", new { Name = "New restaurant" });

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await _client.GetAsync($"v1/restaurants/{id}");
        var restaurant = await response.Content.ReadFromJsonAsync<RestaurantTestDto>(TestData.JsonOptions);

        restaurant!.Name.Should().Be("New restaurant");
    }

    [Fact]
    public async Task ChangeRestaurantDescription_HappyScenario()
    {
        var id = await TestData.SeedRestaurant(_client);

        var result = await _client.PatchAsJsonAsync($"v1/restaurants/{id}/description",
            new { Description = "brand new description" });

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var restaurant = await TestData.GetRestaurant(_client, id);
        restaurant.Description.Should().Be("brand new description");
    }

    [Fact]
    public async Task ChangeRestaurantDescription_FailScenario_TooShort()
    {
        var id = await TestData.SeedRestaurant(_client);

        var result = await _client.PatchAsJsonAsync($"v1/restaurants/{id}/description", new { Description = "short" });

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeRestaurantSchedule_HappyScenario()
    {
        var id = await TestData.SeedRestaurant(_client);
        var newSchedule = new List<OpeningWindow>
            { new(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(18, 0)) };

        var result = await _client.PatchAsJsonAsync($"v1/restaurants/{id}/schedule", new { Schedules = newSchedule });

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var restaurant = await TestData.GetRestaurant(_client, id);
        restaurant.OpeningWindows.Should().ContainSingle(x => x.OpenDay == DayOfWeek.Monday);
    }

    [Fact]
    public async Task DeactivateRestaurant_FailScenario_NoSuchRestaurant()
    {
        var result = await _client.PostAsync($"v1/restaurants/{Guid.NewGuid()}/deactivate", null);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateThenActivateRestaurant_HappyScenario()
    {
        var id = await TestData.SeedRestaurant(_client);

        var deactivate = await _client.PostAsync($"v1/restaurants/{id}/deactivate", null);
        deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDeactivate = await TestData.GetRestaurant(_client, id);
        afterDeactivate.Status.Should().Be(RestaurantStatus.Inactive);

        var activate = await _client.PostAsync($"v1/restaurants/{id}/activate", null);
        activate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterActivate = await TestData.GetRestaurant(_client, id);
        afterActivate.Status.Should().Be(RestaurantStatus.Active);
    }

    [Fact]
    public async Task AddMenuItem_FailScenario_NoSuchRestaurant()
    {
        var request = new AddMenuItemRequest("menu item", "menu item description", Currency.Usd, 5);

        var result = await _client.PostAsJsonAsync($"v1/restaurants/{Guid.NewGuid()}/menu-items", request);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddMenuItem_FailScenario_TooShortName()
    {
        var id = await TestData.SeedRestaurant(_client);
        var request = new AddMenuItemRequest("m", "menu item description", Currency.Usd, 5);

        var result = await _client.PostAsJsonAsync($"v1/restaurants/{id}/menu-items", request);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddMenuItem_HappyScenario()
    {
        var id = await TestData.SeedRestaurant(_client);

        var menuItemId = await TestData.SeedMenuItem(_client, id);

        var restaurant = await TestData.GetRestaurant(_client, id);
        restaurant.MenuItems.Should().ContainSingle(x => x.Id == menuItemId);
    }

    [Fact]
    public async Task RemoveMenuItem_FailScenario_NoSuchItem()
    {
        var id = await TestData.SeedRestaurant(_client);

        var result = await _client.DeleteAsync($"v1/restaurants/{id}/menu-items/{Guid.NewGuid()}");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveMenuItem_HappyScenario()
    {
        var id = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, id);

        var result = await _client.DeleteAsync($"v1/restaurants/{id}/menu-items/{menuItemId}");

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var restaurant = await TestData.GetRestaurant(_client, id);
        restaurant.MenuItems.Should().BeEmpty();
    }

    [Fact]
    public async Task SetMinimalOrderPrice_FailScenario_NegativeAmount()
    {
        var id = await TestData.SeedRestaurant(_client);

        var result = await _client.PatchAsJsonAsync($"v1/restaurants/{id}/minimal-order-price",
            new MoneyRequest(Currency.Usd, -1));

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetMinimalOrderPrice_FailScenario_WrongCurrency()
    {
        var id = await TestData.SeedRestaurant(_client);

        var result = await _client.PatchAsJsonAsync($"v1/restaurants/{id}/minimal-order-price",
            new MoneyRequest(Currency.Eur, 10));

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetMinimalOrderPrice_HappyScenario()
    {
        var id = await TestData.SeedRestaurant(_client);

        var result = await _client.PatchAsJsonAsync($"v1/restaurants/{id}/minimal-order-price",
            new MoneyRequest(Currency.Usd, 42));

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var restaurant = await TestData.GetRestaurant(_client, id);
        restaurant.MinimalOrderPrice.Amount.Should().Be(42);
    }
}