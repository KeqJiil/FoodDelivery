using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;

namespace FoodDelivery.IntegrationTest.Api;

[Collection("Api")]
public class DeliveriesEndpointsTests
{
    private readonly HttpClient _client;
    private readonly FoodDeliveryApiFactory _factory;

    public DeliveriesEndpointsTests(FoodDeliveryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDelivery_FailScenario()
    {
        var result = await _client.GetAsync($"v1/deliveries/{Guid.NewGuid()}");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PickUp_FailScenario_NoSuchDelivery()
    {
        var result = await _client.PostAsync($"v1/deliveries/pickup/{Guid.NewGuid()}", null);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Complete_FailScenario_NoSuchDelivery()
    {
        var result = await _client.PostAsync($"v1/deliveries/complete/{Guid.NewGuid()}", null);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Fail_FailScenario_NoSuchDelivery()
    {
        var result = await _client.PostAsJsonAsync($"v1/deliveries/fail/{Guid.NewGuid()}",
            new { Reason = "courier unavailable" });

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDelivery_HappyScenario()
    {
        var id = await TestData.SeedDelivery(_factory.ConnectionString);

        var result = await _client.GetAsync($"v1/deliveries/{id}");

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PickUp_HappyScenario()
    {
        var id = await TestData.SeedDelivery(_factory.ConnectionString);

        var result = await _client.PostAsync($"v1/deliveries/pickup/{id}", null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Complete_FailScenario_NotPickedUpYet()
    {
        var id = await TestData.SeedDelivery(_factory.ConnectionString);

        var result = await _client.PostAsync($"v1/deliveries/complete/{id}", null);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Complete_HappyScenario()
    {
        var id = await TestData.SeedDelivery(_factory.ConnectionString);
        await _client.PostAsync($"v1/deliveries/pickup/{id}", null);

        var result = await _client.PostAsync($"v1/deliveries/complete/{id}", null);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Fail_HappyScenario()
    {
        var id = await TestData.SeedDelivery(_factory.ConnectionString);

        var result = await _client.PostAsJsonAsync($"v1/deliveries/fail/{id}",
            new { Reason = "courier unavailable" });

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PickUp_FailScenario_AlreadyDelivered()
    {
        var id = await TestData.SeedDelivery(_factory.ConnectionString);
        await _client.PostAsync($"v1/deliveries/pickup/{id}", null);
        await _client.PostAsync($"v1/deliveries/complete/{id}", null);

        var result = await _client.PostAsync($"v1/deliveries/pickup/{id}", null);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Fail_FailScenario_AlreadyDelivered()
    {
        var id = await TestData.SeedDelivery(_factory.ConnectionString);
        await _client.PostAsync($"v1/deliveries/pickup/{id}", null);
        await _client.PostAsync($"v1/deliveries/complete/{id}", null);

        var result = await _client.PostAsJsonAsync($"v1/deliveries/fail/{id}", new { Reason = "too late" });

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
