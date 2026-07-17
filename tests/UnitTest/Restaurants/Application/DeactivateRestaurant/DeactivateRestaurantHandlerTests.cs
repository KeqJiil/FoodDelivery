using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.DeactivateRestaurant;
using Restaurants.Domain.Ids;
using Restaurants.UnitTest.TestHelpers;

namespace Restaurants.UnitTest.Application.DeactivateRestaurant;

public class DeactivateRestaurantHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly DeactivateRestaurantHandler _handler;

    public DeactivateRestaurantHandlerTests()
    {
        _handler = new DeactivateRestaurantHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<DeactivateRestaurantHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldDeactivateRestaurant_WhenFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new DeactivateRestaurantCommand(restaurant.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.IsActive().Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var id = new RestaurantId();
        _repository.Setup(r => r.GetById(id, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Aggregates.Restaurant?)null);

        var result = await _handler.Handle(new DeactivateRestaurantCommand(id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
