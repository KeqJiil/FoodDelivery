using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.CreateRestaurant;
using Restaurants.Domain.Aggregates;
using SharedKernel.Domain.Enums;

namespace Restaurants.UnitTest.Application.CreateRestaurant;

public class CreateRestaurantHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly CreateRestaurantHandler _handler;

    public CreateRestaurantHandlerTests()
    {
        _handler = new CreateRestaurantHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<CreateRestaurantHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldCreateRestaurant_AndPersistIt()
    {
        var command = new CreateRestaurantCommand("Pizzeria", "Best pizza in town", Currency.Usd, 10m, null);
        Restaurant? added = null;
        _repository.Setup(r => r.Add(It.IsAny<Restaurant>())).Callback<Restaurant>(r => added = r);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added.Should().NotBeNull();
        added!.Name.Data.Should().Be(command.Name);
        added.Description.Data.Should().Be(command.Description);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNameIsInvalid()
    {
        var command = new CreateRestaurantCommand("ab", "Best pizza in town", Currency.Usd, 10m, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _repository.Verify(r => r.Add(It.IsAny<Restaurant>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
