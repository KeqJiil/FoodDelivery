using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.ChangeRestaurantSchedule;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using Restaurants.UnitTest.TestHelpers;

namespace Restaurants.UnitTest.Application.ChangeRestaurantSchedule;

public class ChangeRestaurantScheduleHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ChangeRestaurantScheduleHandler _handler;

    public ChangeRestaurantScheduleHandlerTests()
    {
        _handler = new ChangeRestaurantScheduleHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ChangeRestaurantScheduleHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldChangeSchedule_WhenRestaurantFound()
    {
        var restaurant = RestaurantFactory.CreateValid();
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);
        var newSchedule = new List<OpeningWindow>
        {
            new(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0))
        };

        var result = await _handler.Handle(new ChangeRestaurantScheduleCommand(restaurant.Id, newSchedule), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.Schedule.OpeningWindows.Should().BeEquivalentTo(newSchedule);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var id = new RestaurantId();
        _repository.Setup(r => r.GetById(id, It.IsAny<CancellationToken>())).ReturnsAsync((Restaurants.Domain.Aggregates.Restaurant?)null);

        var result = await _handler.Handle(new ChangeRestaurantScheduleCommand(id, []), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
