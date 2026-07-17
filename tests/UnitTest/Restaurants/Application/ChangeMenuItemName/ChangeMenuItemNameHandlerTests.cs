using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.ChangeMenuItemName;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using Restaurants.UnitTest.TestHelpers;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.UnitTest.Application.ChangeMenuItemName;

public class ChangeMenuItemNameHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ChangeMenuItemNameHandler _handler;

    public ChangeMenuItemNameHandlerTests()
    {
        _handler = new ChangeMenuItemNameHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ChangeMenuItemNameHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldChangeMenuItemName_WhenFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!, Description.Create("Classic tomato and mozzarella").Ok!,
            Money.Create(Currency.Usd, 12m).Ok!);
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ChangeMenuItemNameCommand(restaurant.Id, menuItemId, "Diavola"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Single().Name.Data.Should().Be("Diavola");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMenuItemNotFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ChangeMenuItemNameCommand(restaurant.Id, new MenuItemId(), "Diavola"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
