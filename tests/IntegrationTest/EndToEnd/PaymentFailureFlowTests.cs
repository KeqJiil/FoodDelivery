using System.Net.Http.Json;
using Api.Controllers.Restaurants;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Ordering.Domain.Enums;
using OrderRequests.Domain.Enums;
using Payments.Domain.Enums;
using SharedKernel.Domain.Enums;

namespace FoodDelivery.IntegrationTest.EndToEnd;

[Collection("Api")]
public class PaymentFailureFlowTests
{
    private readonly HttpClient _client;
    private readonly FoodDeliveryApiFactory _factory;

    public PaymentFailureFlowTests(FoodDeliveryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PaymentFailure_CompensatesAndFailsOrderWithoutDelivery()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId,
            new AddMenuItemRequest("doomed item", "always fails payment", Currency.Usd, 5.13m));
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });
        await _client.PostAsync($"v1/ordering/{orderId}/place", null);

        OrderRequestTestDto? orderRequest = null;
        await Eventually.AssertAsync(async () =>
        {
            orderRequest = await TestData.FindOrderRequestByOrderId(_client, restaurantId, orderId);
            orderRequest.Should().NotBeNull();
        }, timeout: TimeSpan.FromSeconds(30));

        var approveResult = await _client.PostAsync($"v1/orderrequests/{orderRequest!.OrderRequestId}/approve", null);
        approveResult.EnsureSuccessStatusCode();

        await Eventually.AssertAsync(async () =>
        {
            var payment = await TestData.FindPaymentByOrderId(_factory.ConnectionString, orderId);
            payment.Should().NotBeNull();
            payment!.Status.Should().Be(PaymentStatus.Failed);
        }, timeout: TimeSpan.FromSeconds(30));

        await Eventually.AssertAsync(async () =>
        {
            var updatedRequest = await TestData.FindOrderRequestByOrderId(_client, restaurantId, orderId);
            updatedRequest!.Status.Should().Be(OrderRequestStatus.Cancelled);
        }, timeout: TimeSpan.FromSeconds(30));

        await Eventually.AssertAsync(async () =>
        {
            var order = await TestData.GetOrder(_client, orderId);
            order.Status.Should().Be(OrderStatus.Failed);
        }, timeout: TimeSpan.FromSeconds(30));

        await Eventually.AssertAsync(async () =>
        {
            var sagaState = await TestData.GetSagaState(_factory.ConnectionString, orderId);
            sagaState.Should().BeNull();
        }, timeout: TimeSpan.FromSeconds(30));

        var delivery = await TestData.FindDeliveryByOrderId(_factory.ConnectionString, orderId);
        delivery.Should().BeNull();
    }
}
