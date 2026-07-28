using FluentAssertions;
using Moq;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Adapters;
using Restaurants.Application.Abstractions;
using Restaurants.Application.GetRestaurantById;
using Restaurants.Domain.Enums;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.UnitTest.Infrastructure.Adapters;

public class RestaurantMinimumOrderPriceAdapterTests
{
    private readonly Mock<IRestaurantReader> _reader = new();

    private readonly RestaurantMinimumOrderPriceAdapter _adapter;

    public RestaurantMinimumOrderPriceAdapterTests()
    {
        _adapter = new RestaurantMinimumOrderPriceAdapter(_reader.Object);
    }

    private static RestaurantDto Dto(Guid id, Money minimalOrderPrice) =>
        new(id, "Pizzeria", "Best pizza in town", minimalOrderPrice, RestaurantStatus.Active, [], []);

    [Fact]
    public async Task GetMinimumPriceForOrderAsync_ShouldReturnMinimalOrderPrice_WhenRestaurantExists()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        var price = Money.Create(Currency.Usd, 25.50m).Ok!;
        _reader.Setup(r => r.GetByIdAsync(id.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Dto(id.Id, price));

        var result = await _adapter.GetMinimumPriceForOrderAsync(id);

        result.Should().Be(price);
    }

    [Fact]
    public async Task GetMinimumPriceForOrderAsync_ShouldReturnNull_WhenRestaurantDoesNotExist()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        _reader.Setup(r => r.GetByIdAsync(id.Id, It.IsAny<CancellationToken>())).ReturnsAsync((RestaurantDto?)null);

        var result = await _adapter.GetMinimumPriceForOrderAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMinimumPriceForOrderAsync_ShouldUnwrapTypedId_WhenQueryingReader()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        _reader.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantDto?)null);

        await _adapter.GetMinimumPriceForOrderAsync(id);

        _reader.Verify(r => r.GetByIdAsync(id.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMinimumPriceForOrderAsync_ShouldReturnZeroPrice_WhenRestaurantHasNoMinimum()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        var zero = Money.Create(Currency.Usd, 0m).Ok!;
        _reader.Setup(r => r.GetByIdAsync(id.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Dto(id.Id, zero));

        var result = await _adapter.GetMinimumPriceForOrderAsync(id);

        result.Should().NotBeNull();
        result!.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task GetMinimumPriceForOrderAsync_ShouldPropagateCancellationToken()
    {
        var id = new RestaurantRefId(Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        _reader.Setup(r => r.GetByIdAsync(id.Id, cts.Token)).ReturnsAsync((RestaurantDto?)null);

        await _adapter.GetMinimumPriceForOrderAsync(id, cts.Token);

        _reader.Verify(r => r.GetByIdAsync(id.Id, cts.Token), Times.Once);
    }
}
