using System.Net;
using System.Net.Http.Json;
using Api.Controllers.Restaurants;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using SharedKernel.Domain.Enums;

namespace FoodDelivery.IntegrationTest.EndToEnd;

[Collection("Api")]
public class MinimumOrderPriceFlowTests
{
    private readonly HttpClient _client;
    private readonly FoodDeliveryApiFactory _factory;

    public MinimumOrderPriceFlowTests(FoodDeliveryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task BelowMinimumPrice_OrderRejectedAndSagaNeverStarts()
    {
        var restaurantId = await TestData.SeedRestaurant(_client); // MinimalOrderPrice = 1
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId,
            new AddMenuItemRequest("cheap item", "cheap item description", Currency.Usd, 0.5m));
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });

        var placeResult = await _client.PostAsync($"v1/ordering/{orderId}/place", null);
        placeResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await Task.Delay(1000);
        var sagaState = await TestData.GetSagaState(_factory.ConnectionString, orderId);
        sagaState.Should().BeNull();
    }

    [Fact]
    public async Task ExactlyMinimumPrice_OrderPlacedAndSagaStarts()
    {
        var restaurantId = await TestData.SeedRestaurant(_client); // MinimalOrderPrice = 1
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId,
            new AddMenuItemRequest("exact price item", "exact price item description", Currency.Usd, 1m));
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });

        var placeResult = await _client.PostAsync($"v1/ordering/{orderId}/place", null);
        placeResult.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await Eventually.AssertAsync(async () =>
        {
            var sagaState = await TestData.GetSagaState(_factory.ConnectionString, orderId);
            sagaState.Should().NotBeNull();
            sagaState!.CurrentState.Should().Be("AwaitingApproval");
        }, timeout: TimeSpan.FromSeconds(30));
    }
}
