using System.Net;
using System.Net.Http.Json;
using Api.Controllers.Restaurants;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Ordering.Domain.Enums;
using SharedKernel.Domain.Enums;

namespace FoodDelivery.IntegrationTest.Api;

[Collection("Api")]
public class OrderingEndpointsTests
{
    private readonly HttpClient _client;

    public OrderingEndpointsTests(FoodDeliveryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task InvalidGuidInRoute_ReturnsNotFound()
    {
        var result = await _client.GetAsync("v1/ordering/not-a-guid");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateOrder_HappyScenario()
    {
        var result = await _client.PostAsJsonAsync("v1/ordering", new { RestaurantId = Guid.NewGuid() });

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_FailScenario()
    {
        var result = await _client.GetAsync($"v1/ordering/{Guid.NewGuid()}");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_HappyScenario()
    {
        var id = await TestData.SeedOrder(_client, Guid.NewGuid());

        var result = await _client.GetAsync($"v1/ordering/{id}");

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddOrderLineItem_FailScenario_NoSuchOrder()
    {
        var result = await _client.PostAsJsonAsync($"v1/ordering/{Guid.NewGuid()}/add-items",
            new { MenuId = Guid.NewGuid() });

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddOrderLineItem_FailScenario_NoSuchMenuItem()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var orderId = await TestData.SeedOrder(_client, restaurantId);

        var result = await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items",
            new { MenuId = Guid.NewGuid() });

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddOrderLineItem_HappyScenario()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId);
        var orderId = await TestData.SeedOrder(_client, restaurantId);

        var result = await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });

        result.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await TestData.GetOrder(_client, orderId);
        order.OrderLines.Should().ContainSingle(x => x.MenuItemRefId == menuItemId);
    }

    [Fact]
    public async Task RemoveOrderLineItem_FailScenario_NoSuchOrder()
    {
        var result = await _client.DeleteAsync($"v1/ordering/{Guid.NewGuid()}/remove/{Guid.NewGuid()}");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveOrderLineItem_HappyScenario()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId);
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });
        var order = await TestData.GetOrder(_client, orderId);
        var orderLineId = order.OrderLines.Single().Id;

        var result = await _client.DeleteAsync($"v1/ordering/{orderId}/remove/{orderLineId}");

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updated = await TestData.GetOrder(_client, orderId);
        updated.OrderLines.Should().BeEmpty();
    }

    [Fact]
    public async Task PlaceOrder_FailScenario_NoSuchOrder()
    {
        var result = await _client.PostAsync($"v1/ordering/{Guid.NewGuid()}/place", null);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PlaceOrder_FailScenario_NoSuchRestaurant()
    {
        var orderId = await TestData.SeedOrder(_client, Guid.NewGuid());

        var result = await _client.PostAsync($"v1/ordering/{orderId}/place", null);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PlaceOrder_FailScenario_EmptyOrder()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var orderId = await TestData.SeedOrder(_client, restaurantId);

        var result = await _client.PostAsync($"v1/ordering/{orderId}/place", null);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PlaceOrder_FailScenario_BelowMinimumPrice()
    {
        var restaurantId = await TestData.SeedRestaurant(_client); // MinimalOrderPrice = 1
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId,
            new AddMenuItemRequest("cheap item", "cheap item description", Currency.Usd, 0.5m));
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });

        var result = await _client.PostAsync($"v1/ordering/{orderId}/place", null);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PlaceOrder_HappyScenario()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId);
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });

        var result = await _client.PostAsync($"v1/ordering/{orderId}/place", null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var order = await TestData.GetOrder(_client, orderId);
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task PlaceOrder_FailScenario_AlreadyPlaced()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId);
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });
        await _client.PostAsync($"v1/ordering/{orderId}/place", null);

        var result = await _client.PostAsync($"v1/ordering/{orderId}/place", null);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CancelOrder_FailScenario_NoSuchOrder()
    {
        var result = await _client.PostAsync($"v1/ordering/{Guid.NewGuid()}/cancel", null);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelOrder_HappyScenario()
    {
        var orderId = await TestData.SeedOrder(_client, Guid.NewGuid());

        var result = await _client.PostAsync($"v1/ordering/{orderId}/cancel", null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var order = await TestData.GetOrder(_client, orderId);
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOrder_FailScenario_AlreadyCancelled()
    {
        var orderId = await TestData.SeedOrder(_client, Guid.NewGuid());
        await _client.PostAsync($"v1/ordering/{orderId}/cancel", null);

        var result = await _client.PostAsync($"v1/ordering/{orderId}/cancel", null);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddOrderLineItem_SameMenuItemTwice_IncreasesQuantityInsteadOfDuplicatingLine()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId);
        var orderId = await TestData.SeedOrder(_client, restaurantId);

        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });

        var order = await TestData.GetOrder(_client, orderId);
        order.OrderLines.Should().ContainSingle(x => x.MenuItemRefId == menuItemId)
            .Which.Quantity.Should().Be(2);
    }

    [Fact]
    public async Task RemoveOrderLineItem_WhenQuantityGreaterThanOne_DecreasesInsteadOfRemoving()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId);
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });
        var orderLineId = (await TestData.GetOrder(_client, orderId)).OrderLines.Single().Id;

        var result = await _client.DeleteAsync($"v1/ordering/{orderId}/remove/{orderLineId}");

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var order = await TestData.GetOrder(_client, orderId);
        order.OrderLines.Should().ContainSingle(x => x.MenuItemRefId == menuItemId)
            .Which.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task PlaceOrder_HappyScenario_TotalExactlyEqualsMinimumPrice()
    {
        var restaurantId = await TestData.SeedRestaurant(_client); // MinimalOrderPrice = 1
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId,
            new AddMenuItemRequest("exact price item", "exact price item description", Currency.Usd, 1m));
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });

        var result = await _client.PostAsync($"v1/ordering/{orderId}/place", null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
