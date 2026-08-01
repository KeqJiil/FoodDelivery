using System.Net;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;

namespace FoodDelivery.IntegrationTest.Api;

[Collection("Api")]
public class PaymentsEndpointsTests
{
    private readonly HttpClient _client;

    public PaymentsEndpointsTests(FoodDeliveryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPayment_FailScenario()
    {
        var result = await _client.GetAsync($"v1/Payments/{Guid.NewGuid()}");
        
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}