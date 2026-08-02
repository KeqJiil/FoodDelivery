using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Ordering.Domain.Enums;
using OrderRequests.Domain.Enums;

namespace FoodDelivery.IntegrationTest.EndToEnd;

[Collection("Api")]
public class CancellationFlowTests
{
    private readonly HttpClient _client;
    private readonly FoodDeliveryApiFactory _factory;

    public CancellationFlowTests(FoodDeliveryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Cancellation_BeforeApproval_PropagatesToOrderRequestAndSagaFinalizesWithoutPayment()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId);
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });
        await _client.PostAsync($"v1/ordering/{orderId}/place", null);

        OrderRequestTestDto? orderRequest = null;
        await Eventually.AssertAsync(async () =>
        {
            orderRequest = await TestData.FindOrderRequestByOrderId(_client, restaurantId, orderId);
            orderRequest.Should().NotBeNull();
        }, timeout: TimeSpan.FromSeconds(30));

        var cancelResult = await _client.PostAsync($"v1/ordering/{orderId}/cancel", null);
        cancelResult.EnsureSuccessStatusCode();

        var order = await TestData.GetOrder(_client, orderId);
        order.Status.Should().Be(OrderStatus.Cancelled);

        await Eventually.AssertAsync(async () =>
        {
            var updatedRequest = await TestData.FindOrderRequestByOrderId(_client, restaurantId, orderId);
            updatedRequest!.Status.Should().Be(OrderRequestStatus.Cancelled);
        }, timeout: TimeSpan.FromSeconds(30));

        var payment = await TestData.FindPaymentByOrderId(_factory.ConnectionString, orderId);
        payment.Should().BeNull();
    }
}
