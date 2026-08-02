using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Ordering.Domain.Enums;
using OrderRequests.Domain.Enums;

namespace FoodDelivery.IntegrationTest.EndToEnd;

[Collection("ApprovalTimeout")]
public class ApprovalTimeoutFlowTests
{
    private readonly HttpClient _client;
    private readonly ApprovalTimeoutApiFactory _factory;

    public ApprovalTimeoutFlowTests(ApprovalTimeoutApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ApprovalTimeout_FailsOrderAndCancelsRequestWithoutHumanAction()
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
        orderRequest!.Status.Should().Be(OrderRequestStatus.Pending);

        await Eventually.AssertAsync(async () =>
        {
            var order = await TestData.GetOrder(_client, orderId);
            order.Status.Should().Be(OrderStatus.Failed);
        }, timeout: TimeSpan.FromSeconds(100));

        await Eventually.AssertAsync(async () =>
        {
            var updatedRequest = await TestData.FindOrderRequestByOrderId(_client, restaurantId, orderId);
            updatedRequest!.Status.Should().Be(OrderRequestStatus.Cancelled);
        }, timeout: TimeSpan.FromSeconds(30));

        await Eventually.AssertAsync(async () =>
        {
            var sagaState = await TestData.GetSagaState(_factory.ConnectionString, orderId);
            sagaState.Should().BeNull();
        }, timeout: TimeSpan.FromSeconds(30));
    }
}
