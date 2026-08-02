using System.Net.Http.Json;
using Api.Controllers.Restaurants;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using SharedKernel.Domain.Enums;

namespace FoodDelivery.IntegrationTest.EndToEnd;

[Collection("Api")]
public class MenuPriceChangeFlowTests
{
    private readonly HttpClient _client;

    public MenuPriceChangeFlowTests(FoodDeliveryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PriceChange_UpdatesDraftOrders_ButNotPlacedOrders()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId,
            new AddMenuItemRequest("menu item", "menu item description", Currency.Usd, 5m));

        var draftOrderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{draftOrderId}/add-items", new { MenuId = menuItemId });

        var placedOrderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{placedOrderId}/add-items", new { MenuId = menuItemId });
        await _client.PostAsync($"v1/ordering/{placedOrderId}/place", null);

        var changeResult = await _client.PutAsJsonAsync($"v1/restaurants/{restaurantId}/menu-items/{menuItemId}/price",
            new MoneyRequest(Currency.Usd, 8m));
        changeResult.EnsureSuccessStatusCode();

        await Eventually.AssertAsync(async () =>
        {
            var draftOrder = await TestData.GetOrder(_client, draftOrderId);
            draftOrder.OrderLines.Single().Price.Amount.Should().Be(8m);
        }, timeout: TimeSpan.FromSeconds(30));

        var placedOrder = await TestData.GetOrder(_client, placedOrderId);
        placedOrder.OrderLines.Single().Price.Amount.Should().Be(5m);
    }
}
