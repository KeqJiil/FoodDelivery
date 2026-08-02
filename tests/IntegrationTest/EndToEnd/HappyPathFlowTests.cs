using System.Net.Http.Json;
using Deliveries.Domain.Enums;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Ordering.Domain.Enums;
using OrderRequests.Domain.Enums;
using Payments.Domain.Enums;

namespace FoodDelivery.IntegrationTest.EndToEnd;

[Collection("Api")]
public class HappyPathFlowTests
{
    private readonly HttpClient _client;
    private readonly FoodDeliveryApiFactory _factory;

    public HappyPathFlowTests(FoodDeliveryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HappyPath_OrderReachesConfirmedAndSagaCompletes()
    {
        var restaurantId = await TestData.SeedRestaurant(_client);
        var menuItemId = await TestData.SeedMenuItem(_client, restaurantId);
        var orderId = await TestData.SeedOrder(_client, restaurantId);
        await _client.PostAsJsonAsync($"v1/ordering/{orderId}/add-items", new { MenuId = menuItemId });

        var placeResult = await _client.PostAsync($"v1/ordering/{orderId}/place", null);
        placeResult.EnsureSuccessStatusCode();

        OrderRequestTestDto? orderRequest = null;
        await Eventually.AssertAsync(async () =>
        {
            orderRequest = await TestData.FindOrderRequestByOrderId(_client, restaurantId, orderId);
            orderRequest.Should().NotBeNull();
        }, timeout: TimeSpan.FromSeconds(30));
        orderRequest!.Status.Should().Be(OrderRequestStatus.Pending);

        var approveResult = await _client.PostAsync($"v1/orderrequests/{orderRequest.OrderRequestId}/approve", null);
        approveResult.EnsureSuccessStatusCode();

        Payments.Domain.Aggregates.Payment? payment = null;
        await Eventually.AssertAsync(async () =>
        {
            payment = await TestData.FindPaymentByOrderId(_factory.ConnectionString, orderId);
            payment.Should().NotBeNull();
            payment!.Status.Should().Be(PaymentStatus.Succeeded);
        }, timeout: TimeSpan.FromSeconds(30));

        await Eventually.AssertAsync(async () =>
        {
            var order = await TestData.GetOrder(_client, orderId);
            order.Status.Should().Be(OrderStatus.Confirmed);
        }, timeout: TimeSpan.FromSeconds(30));

        Deliveries.Domain.Aggregates.Delivery? delivery = null;
        await Eventually.AssertAsync(async () =>
        {
            delivery = await TestData.FindDeliveryByOrderId(_factory.ConnectionString, orderId);
            delivery.Should().NotBeNull();
            delivery!.Status.Should().Be(DeliveryStatus.Pending);
        }, timeout: TimeSpan.FromSeconds(30));

        await Eventually.AssertAsync(async () =>
        {
            var sagaState = await TestData.GetSagaState(_factory.ConnectionString, orderId);
            sagaState.Should().BeNull();
        }, timeout: TimeSpan.FromSeconds(30));
    }
}
