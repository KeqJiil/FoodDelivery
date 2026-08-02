using System.Net;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;

namespace FoodDelivery.IntegrationTest.Api;

[Collection("Api")]
public class OrderRequestsEndpointsTests
{
    private readonly HttpClient _client;
    private readonly FoodDeliveryApiFactory _factory;

    public OrderRequestsEndpointsTests(FoodDeliveryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrder_FailScenario()
    {
        var result = await _client.GetAsync($"v1/orderrequests/{Guid.NewGuid()}");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrdersByRestaurantId_FailScenario_NoLimit()
    {
        var result = await _client.GetAsync($"v1/orderrequests/restaurant/{Guid.NewGuid()}");

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrdersByRestaurantId_HappyScenario_EmptyResult()
    {
        var result = await _client.GetAsync($"v1/orderrequests/restaurant/{Guid.NewGuid()}?Limit=10");

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RejectOrder_FailScenario_NoSuchOrder()
    {
        var result = await _client.PostAsync($"v1/orderrequests/{Guid.NewGuid()}/reject", null);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApproveOrder_FailScenario_NoSuchOrder()
    {
        var result = await _client.PostAsync($"v1/orderrequests/{Guid.NewGuid()}/approve", null);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrder_HappyScenario()
    {
        var id = await TestData.SeedOrderRequest(_factory.ConnectionString);

        var result = await _client.GetAsync($"v1/orderrequests/{id}");

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ApproveOrder_HappyScenario()
    {
        var id = await TestData.SeedOrderRequest(_factory.ConnectionString);

        var result = await _client.PostAsync($"v1/orderrequests/{id}/approve", null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ApproveOrder_FailScenario_AlreadyApproved()
    {
        var id = await TestData.SeedOrderRequest(_factory.ConnectionString);
        await _client.PostAsync($"v1/orderrequests/{id}/approve", null);

        var result = await _client.PostAsync($"v1/orderrequests/{id}/approve", null);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RejectOrder_HappyScenario()
    {
        var id = await TestData.SeedOrderRequest(_factory.ConnectionString);

        var result = await _client.PostAsync($"v1/orderrequests/{id}/reject", null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RejectOrder_FailScenario_AlreadyApproved()
    {
        var id = await TestData.SeedOrderRequest(_factory.ConnectionString);
        await _client.PostAsync($"v1/orderrequests/{id}/approve", null);

        var result = await _client.PostAsync($"v1/orderrequests/{id}/reject", null);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ApproveOrder_FailScenario_AlreadyRejected()
    {
        var id = await TestData.SeedOrderRequest(_factory.ConnectionString);
        await _client.PostAsync($"v1/orderrequests/{id}/reject", null);

        var result = await _client.PostAsync($"v1/orderrequests/{id}/approve", null);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
