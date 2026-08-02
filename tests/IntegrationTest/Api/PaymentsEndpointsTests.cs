using System.Net;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;

namespace FoodDelivery.IntegrationTest.Api;

[Collection("Api")]
public class PaymentsEndpointsTests
{
    private readonly HttpClient _client;
    private readonly FoodDeliveryApiFactory _factory;

    public PaymentsEndpointsTests(FoodDeliveryApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPayment_FailScenario()
    {
        var result = await _client.GetAsync($"v1/Payments/{Guid.NewGuid()}");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPayment_HappyScenario()
    {
        var id = await TestData.SeedPayment(_factory.ConnectionString);

        var result = await _client.GetAsync($"v1/Payments/{id}");

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}