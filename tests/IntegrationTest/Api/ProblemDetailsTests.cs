using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;

namespace FoodDelivery.IntegrationTest.Api;

[Collection("Api")]
public class ProblemDetailsTests
{
    private readonly HttpClient _client;

    public ProblemDetailsTests(FoodDeliveryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task NotFoundError_ReturnsProblemDetailsShape()
    {
        var response = await _client.GetAsync($"v1/restaurants/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task ValidationError_ReturnsProblemDetailsWithFilledFields()
    {
        var id = await TestData.SeedRestaurant(_client);
        
        var response = await _client.PostAsJsonAsync($"v1/restaurants/{id}/menu-items",
            new { Name = "menu item", Description = "menu item description", Currency = "Eur", Amount = 5m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();
        problem!.Title.Should().Be("Bad Request");
        problem.Status.Should().Be(400);
        problem.Detail.Should().NotBeNullOrWhiteSpace();
    }
}

internal record ProblemDetailsDto(string Title, int Status, string Detail);
