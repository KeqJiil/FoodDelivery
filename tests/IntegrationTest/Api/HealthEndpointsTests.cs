using System.Net;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;

namespace FoodDelivery.IntegrationTest.Api;

[Collection("Api")]
public class HealthEndpointsTests
{
    private HttpClient _client;

    public HealthEndpointsTests(FoodDeliveryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CheckLiveness()
    {
        var result = await _client.GetAsync("/liveness");

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CheckReadiness()
    {
        await Eventually.AssertAsync(async () =>
        {
            var result = await _client.GetAsync("/readiness");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        });
    }
}