using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.RemoveMenuItem;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using Restaurants.UnitTest.TestHelpers;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.UnitTest.Application.RemoveMenuItem;

public class RemoveMenuItemHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly RemoveMenuItemHandler _handler;

    public RemoveMenuItemHandlerTests()
    {
        _handler = new RemoveMenuItemHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RemoveMenuItemHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldRemoveMenuItem_WhenFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        var menuItemId = new MenuItemId();
        restaurant.AddMenuItem(menuItemId, Name.Create("Margherita").Ok!, Description.Create("Classic tomato and mozzarella").Ok!,
            Money.Create(Currency.Usd, 12m).Ok!);
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new RemoveMenuItemCommand(restaurant.Id, menuItemId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Should().BeEmpty();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMenuItemNotFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new RemoveMenuItemCommand(restaurant.Id, new MenuItemId()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var id = new RestaurantId();
        _repository.Setup(r => r.GetById(id, It.IsAny<CancellationToken>())).ReturnsAsync((Restaurants.Domain.Aggregates.Restaurant?)null);

        var result = await _handler.Handle(new RemoveMenuItemCommand(id, new MenuItemId()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
