using FluentAssertions;
using Moq;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Adapters;
using Restaurants.Application.Abstractions;

namespace Ordering.UnitTest.Infrastructure.Adapters;

public class RestaurantActiveAdapterTests
{
    private readonly Mock<IRestaurantReader> _reader = new();

    private readonly RestaurantActiveAdapter _adapter;

    public RestaurantActiveAdapterTests()
    {
        _adapter = new RestaurantActiveAdapter(_reader.Object);
    }

    [Fact]
    public async Task IsActiveAsync_ShouldReturnTrue_WhenRestaurantIsActive()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        _reader.Setup(r => r.IsActiveAsync(id.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _adapter.IsActiveAsync(id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsActiveAsync_ShouldReturnFalse_WhenRestaurantIsInactive()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        _reader.Setup(r => r.IsActiveAsync(id.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _adapter.IsActiveAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsActiveAsync_ShouldReturnFalse_WhenRestaurantDoesNotExist()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        _reader.Setup(r => r.IsActiveAsync(id.Id, It.IsAny<CancellationToken>())).ReturnsAsync((bool?)null);

        var result = await _adapter.IsActiveAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsActiveAsync_ShouldUnwrapTypedId_WhenQueryingReader()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        _reader.Setup(r => r.IsActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null);

        await _adapter.IsActiveAsync(id);

        _reader.Verify(r => r.IsActiveAsync(id.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsActiveAsync_ShouldPropagateCancellationToken()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        _reader.Setup(r => r.IsActiveAsync(id.Id, cts.Token)).ReturnsAsync((bool?)null);

        await _adapter.IsActiveAsync(id, cts.Token);

        _reader.Verify(r => r.IsActiveAsync(id.Id, cts.Token), Times.Once);
    }
}
