using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.ChangeRestaurantName;
using Restaurants.Domain.Ids;
using Restaurants.UnitTest.TestHelpers;

namespace Restaurants.UnitTest.Application.ChangeRestaurantName;

public class ChangeRestaurantNameHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ChangeRestaurantNameHandler _handler;

    public ChangeRestaurantNameHandlerTests()
    {
        _handler = new ChangeRestaurantNameHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ChangeRestaurantNameHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldChangeName_WhenValid()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ChangeRestaurantNameCommand(restaurant.Id, "New Name"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.Name.Data.Should().Be("New Name");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNameInvalid()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new ChangeRestaurantNameCommand(restaurant.Id, "x"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var id = new RestaurantId();
        _repository.Setup(r => r.GetById(id, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Aggregates.Restaurant?)null);

        var result = await _handler.Handle(new ChangeRestaurantNameCommand(id, "New Name"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
