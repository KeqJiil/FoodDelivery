using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.ChangeMenuItemDescription;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using Restaurants.UnitTest.TestHelpers;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.UnitTest.Application.ChangeMenuItemDescription;

public class ChangeMenuItemDescriptionHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ChangeMenuItemDescriptionHandler _handler;

    public ChangeMenuItemDescriptionHandlerTests()
    {
        _handler = new ChangeMenuItemDescriptionHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ChangeMenuItemDescriptionHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldChangeMenuItemDescription_WhenFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!, Description.Create("Classic tomato and mozzarella").Ok!,
            Money.Create(Currency.Usd, 12m).Ok!);
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);
        const string newDescription = "Spicy salami and mozzarella cheese";

        var result = await _handler.Handle(new ChangeMenuItemDescriptionCommand(restaurant.Id, menuItemId, newDescription), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Single().Description.Data.Should().Be(newDescription);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMenuItemNotFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ChangeMenuItemDescriptionCommand(restaurant.Id, new MenuItemId(), "Spicy salami and mozzarella cheese"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
