using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.ActivateRestaurant;
using Restaurants.Domain.Ids;
using Restaurants.UnitTest.TestHelpers;

namespace Restaurants.UnitTest.Application.ActivateRestaurant;

public class ActivateRestaurantHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ActivateRestaurantHandler _handler;

    public ActivateRestaurantHandlerTests()
    {
        _handler = new ActivateRestaurantHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ActivateRestaurantHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldActivateRestaurant_WhenFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        restaurant.Deactivate();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ActivateRestaurantCommand(restaurant.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.IsActive().Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var id = new RestaurantId();
        _repository.Setup(r => r.GetById(id, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Aggregates.Restaurant?)null);

        var result = await _handler.Handle(new ActivateRestaurantCommand(id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
