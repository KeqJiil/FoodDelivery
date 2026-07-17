using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.ChangeMenuItemPrice;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using Restaurants.UnitTest.TestHelpers;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.UnitTest.Application.ChangeMenuItemPrice;

public class ChangeMenuItemPriceHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ChangeMenuItemPriceHandler _handler;

    public ChangeMenuItemPriceHandlerTests()
    {
        _handler = new ChangeMenuItemPriceHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ChangeMenuItemPriceHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldChangePrice_WhenSameCurrency()
    {
        var restaurant = RestaurantFactory.CreateValid(currency: Currency.Usd);
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!, Description.Create("Classic tomato and mozzarella").Ok!,
            Money.Create(Currency.Usd, 12m).Ok!);
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ChangeMenuItemPriceCommand(restaurant.Id, menuItemId, Currency.Usd, 15m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Single().Price.Amount.Should().Be(15m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCurrencyMismatches()
    {
        var restaurant = RestaurantFactory.CreateValid(currency: Currency.Usd);
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!, Description.Create("Classic tomato and mozzarella").Ok!,
            Money.Create(Currency.Usd, 12m).Ok!);
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ChangeMenuItemPriceCommand(restaurant.Id, menuItemId, Currency.Eur, 15m), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        restaurant.MenuItems.Single().Price.Amount.Should().Be(12m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMenuItemNotFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ChangeMenuItemPriceCommand(restaurant.Id, new MenuItemId(), Currency.Usd, 15m), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
