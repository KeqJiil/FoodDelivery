using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.ChangeRestaurantDescription;
using Restaurants.Domain.Ids;
using Restaurants.UnitTest.TestHelpers;

namespace Restaurants.UnitTest.Application.ChangeRestaurantDescription;

public class ChangeRestaurantDescriptionHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ChangeRestaurantDescriptionHandler _handler;

    public ChangeRestaurantDescriptionHandlerTests()
    {
        _handler = new ChangeRestaurantDescriptionHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ChangeRestaurantDescriptionHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldChangeDescription_WhenValid()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);
        const string newDescription = "A brand new description of this place";

        var result = await _handler.Handle(new ChangeRestaurantDescriptionCommand(restaurant.Id, newDescription), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.Description.Data.Should().Be(newDescription);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDescriptionInvalid()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ChangeRestaurantDescriptionCommand(restaurant.Id, "short"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var id = new RestaurantId();
        _repository.Setup(r => r.GetById(id, It.IsAny<CancellationToken>())).ReturnsAsync((Restaurants.Domain.Aggregates.Restaurant?)null);

        var result = await _handler.Handle(new ChangeRestaurantDescriptionCommand(id, "A brand new description of this place"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
