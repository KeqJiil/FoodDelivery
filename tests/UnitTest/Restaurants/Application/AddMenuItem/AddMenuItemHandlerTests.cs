using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.AddMenuItem;
using Restaurants.Domain.Ids;
using Restaurants.UnitTest.TestHelpers;
using SharedKernel.Domain.Enums;

namespace Restaurants.UnitTest.Application.AddMenuItem;

public class AddMenuItemHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly AddMenuItemHandler _handler;

    public AddMenuItemHandlerTests()
    {
        _handler = new AddMenuItemHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AddMenuItemHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldAddMenuItem_WhenRestaurantFound()
    {
        var restaurant = RestaurantFactory.CreateValid(currency: Currency.Usd);
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);
        var command = new AddMenuItemCommand(restaurant.Id, "Margherita", "Classic tomato and mozzarella", Currency.Usd, 12m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.MenuItems.Should().ContainSingle(m => m.Id == result.Ok);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCurrencyMismatches()
    {
        var restaurant = RestaurantFactory.CreateValid(currency: Currency.Usd);
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);
        var command = new AddMenuItemCommand(restaurant.Id, "Margherita", "Classic tomato and mozzarella", Currency.Eur, 12m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        restaurant.MenuItems.Should().BeEmpty();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var id = new RestaurantId();
        _repository.Setup(r => r.GetById(id, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Aggregates.Restaurant?)null);
        var command = new AddMenuItemCommand(id, "Margherita", "Classic tomato and mozzarella", Currency.Usd, 12m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
