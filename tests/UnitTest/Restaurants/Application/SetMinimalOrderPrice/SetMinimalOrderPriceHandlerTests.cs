using FluentAssertions;
using Moq;
using Restaurants.Application.Abstractions;
using Restaurants.Application.SetMinimalOrderPrice;
using Restaurants.Domain.Ids;
using Restaurants.UnitTest.TestHelpers;
using SharedKernel.Domain.Enums;

namespace Restaurants.UnitTest.Application.SetMinimalOrderPrice;

public class SetMinimalOrderPriceHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly SetMinimalOrderPriceHandler _handler;

    public SetMinimalOrderPriceHandlerTests()
    {
        _handler = new SetMinimalOrderPriceHandler(_repository.Object, _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SetMinimalOrderPriceHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldSetPrice_WhenSameCurrency()
    {
        var restaurant = RestaurantFactory.CreateValid(currency: Currency.Usd);
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new SetMinimalOrderPriceCommand(restaurant.Id, Currency.Usd, 25m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        restaurant.MinimalOrderPrice.Amount.Should().Be(25m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCurrencyMismatches()
    {
        var restaurant = RestaurantFactory.CreateValid(currency: Currency.Usd);
        _repository.Setup(r => r.GetById(restaurant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(restaurant);

        var result = await _handler.Handle(new SetMinimalOrderPriceCommand(restaurant.Id, Currency.Eur, 25m), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRestaurantNotFound()
    {
        var id = new RestaurantId();
        _repository.Setup(r => r.GetById(id, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Aggregates.Restaurant?)null);

        var result = await _handler.Handle(new SetMinimalOrderPriceCommand(id, Currency.Usd, 25m), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
