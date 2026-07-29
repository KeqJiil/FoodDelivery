using FluentAssertions;
using Moq;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Adapters;
using Restaurants.Application.Abstractions;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.UnitTest.Infrastructure.Adapters;

public class MenuPriceForOrderLineAdapterTests
{
    private readonly Mock<IRestaurantReader> _reader = new();

    private readonly MenuPriceForOrderLineAdapter _adapter;

    public MenuPriceForOrderLineAdapterTests()
    {
        _adapter = new MenuPriceForOrderLineAdapter(_reader.Object);
    }

    [Fact]
    public async Task GetMenuItemPriceByIdAsync_ShouldReturnPrice_WhenMenuItemExists()
    {
        var id = new MenuItemRefId(Guid.NewGuid());
        var price = Money.Create(Currency.Usd, 12.5m).Ok!;
        _reader.Setup(r => r.GetMenuItemPriceByIdAsync(id.Id, It.IsAny<CancellationToken>())).ReturnsAsync(price);

        var result = await _adapter.GetMenuItemPriceByIdAsync(id);

        result.Should().Be(price);
    }

    [Fact]
    public async Task GetMenuItemPriceByIdAsync_ShouldReturnNull_WhenMenuItemDoesNotExist()
    {
        var id = new MenuItemRefId(Guid.NewGuid());
        _reader.Setup(r => r.GetMenuItemPriceByIdAsync(id.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Money?)null);

        var result = await _adapter.GetMenuItemPriceByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMenuItemPriceByIdAsync_ShouldUnwrapTypedId_WhenQueryingReader()
    {
        var id = new MenuItemRefId(Guid.NewGuid());
        _reader.Setup(r => r.GetMenuItemPriceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Money?)null);

        await _adapter.GetMenuItemPriceByIdAsync(id);

        _reader.Verify(r => r.GetMenuItemPriceByIdAsync(id.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMenuItemPriceByIdAsync_ShouldReturnZeroPrice_WhenMenuItemIsFree()
    {
        var id = new MenuItemRefId(Guid.NewGuid());
        var zero = Money.Create(Currency.Usd, 0m).Ok!;
        _reader.Setup(r => r.GetMenuItemPriceByIdAsync(id.Id, It.IsAny<CancellationToken>())).ReturnsAsync(zero);

        var result = await _adapter.GetMenuItemPriceByIdAsync(id);

        result.Should().NotBeNull();
        result!.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task GetMenuItemPriceByIdAsync_ShouldPropagateCancellationToken()
    {
        var id = new MenuItemRefId(Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        _reader.Setup(r => r.GetMenuItemPriceByIdAsync(id.Id, cts.Token)).ReturnsAsync((Money?)null);

        await _adapter.GetMenuItemPriceByIdAsync(id, cts.Token);

        _reader.Verify(r => r.GetMenuItemPriceByIdAsync(id.Id, cts.Token), Times.Once);
    }
}
