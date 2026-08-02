using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;

namespace FoodDelivery.IntegrationTest.Api;

[Collection("Api")]
public class JsonContractTests
{
    private readonly HttpClient _client;

    public JsonContractTests(FoodDeliveryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRestaurant_EnumsAreSerializedAsStrings()
    {
        var id = await TestData.SeedRestaurant(_client);

        var response = await _client.GetAsync($"v1/restaurants/{id}");
        var raw = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(raw);
        doc.RootElement.GetProperty("status").ValueKind.Should().Be(JsonValueKind.String);
        doc.RootElement.GetProperty("status").GetString().Should().Be("Active");
    }

    [Fact]
    public async Task CreateRestaurant_UnknownEnumValue_ReturnsBadRequest()
    {
        var payload = """
                      {
                        "name": "restaurant",
                        "description": "restaurant",
                        "amount": 1,
                        "currency": "NotARealCurrency",
                        "schedules": [
                          { "openDay": "Friday", "openTime": "01:00", "closeDay": "Monday", "closeTime": "02:00" }
                        ]
                      }
                      """;
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("v1/restaurants", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetMinimalOrderPrice_DecimalPrecisionIsPreserved()
    {
        var id = await TestData.SeedRestaurant(_client);

        var patch = await _client.PatchAsJsonAsync($"v1/restaurants/{id}/minimal-order-price",
            new { Currency = "Usd", Amount = 19.99m });
        patch.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var restaurant = await TestData.GetRestaurant(_client, id);
        restaurant.MinimalOrderPrice.Amount.Should().Be(19.99m);
    }

    [Fact]
    public async Task Preflight_ReturnsCorsHeaders()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "v1/restaurants");
        request.Headers.Add("Origin", "https://example.com");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await _client.SendAsync(request);

        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
    }
}
