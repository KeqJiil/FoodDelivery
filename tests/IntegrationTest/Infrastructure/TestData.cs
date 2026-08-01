using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Controllers.Restaurants;
using Restaurants.Domain.Enums;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain.Enums;

namespace FoodDelivery.IntegrationTest.Infrastructure;

internal record CreatedResponse(Guid Id);

internal record MoneyTestDto(Currency Currency, decimal Amount);

internal record MenuItemTestDto(Guid Id, string Name, string Description, MoneyTestDto Price);

internal record RestaurantTestDto(
    Guid Id,
    string Name,
    string Description,
    MoneyTestDto MinimalOrderPrice,
    RestaurantStatus Status,
    IReadOnlyList<OpeningWindow> OpeningWindows,
    IReadOnlyList<MenuItemTestDto> MenuItems);

public static class TestData
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static CreateRestaurantRequest GetRestaurantRequest()
    {
        return new CreateRestaurantRequest("restaurant", "restaurant", 1, Currency.Usd,
            new List<OpeningWindow>
                { new(DayOfWeek.Friday, new TimeOnly(1, 1), DayOfWeek.Monday, new TimeOnly(2, 2)) });
    }

    public static async Task<Guid> SeedRestaurant(HttpClient client,
        CreateRestaurantRequest? createRestaurantRequest = null)
    {
        var restaurant = createRestaurantRequest ?? GetRestaurantRequest();

        var response = await client.PostAsJsonAsync("v1/restaurants", restaurant);
        var body = await response.Content.ReadFromJsonAsync<CreatedResponse>();

        return body!.Id;
    }

    internal static async Task<RestaurantTestDto> GetRestaurant(HttpClient client, Guid restaurantId)
    {
        var response = await client.GetAsync($"v1/restaurants/{restaurantId}");

        return (await response.Content.ReadFromJsonAsync<RestaurantTestDto>(JsonOptions))!;
    }
    
    public static async Task<Guid> SeedMenuItem(HttpClient client, Guid restaurantId,
        AddMenuItemRequest? addMenuItemRequest = null)
    {
        var menuItem = addMenuItemRequest ??
                       new AddMenuItemRequest("menu item", "menu item description", Currency.Usd, 5);

        await client.PostAsJsonAsync($"v1/restaurants/{restaurantId}/menu-items", menuItem);

        var restaurant = await GetRestaurant(client, restaurantId);

        return restaurant.MenuItems.First(x => x.Name == menuItem.Name).Id;
    }
}
