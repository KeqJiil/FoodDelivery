using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.GetRestaurantById;
using Restaurants.Domain.Enums;

namespace Restaurants.UnitTest.Application.GetRestaurantById;

public class GetRestaurantByIdHandlerTests
{
    private readonly Mock<IRestaurantReader> _reader = new();

    private readonly GetRestaurantByIdHandler _handler;

    public GetRestaurantByIdHandlerTests()
    {
        _handler = new GetRestaurantByIdHandler(_reader.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenRestaurantFound()
    {
        var id = Guid.NewGuid();
        var dto = new RestaurantDto(id, "Pizzeria", "Best pizza in town", null!, RestaurantStatus.Active, [], []);
        _reader.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _handler.Handle(new GetRestaurantByIdQuery(id), CancellationToken.None);

        result.Should().Be(dto);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRestaurantNotFound()
    {
        var id = Guid.NewGuid();
        _reader.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((RestaurantDto?)null);

        var result = await _handler.Handle(new GetRestaurantByIdQuery(id), CancellationToken.None);

        result.Should().BeNull();
    }
}
