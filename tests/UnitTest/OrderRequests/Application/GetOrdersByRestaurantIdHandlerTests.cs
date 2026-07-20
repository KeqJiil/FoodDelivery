using FluentAssertions;
using Moq;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Application.GetOrdersByRestaurantId;
using OrderRequests.Domain.Enums;

namespace OrderRequests.UnitTest.Application;

public class GetOrdersByRestaurantIdHandlerTests
{
    private Mock<IOrderRequestReader> _reader = new();

    private readonly GetOrdersByRestaurantIdHandler _handler;

    public GetOrdersByRestaurantIdHandlerTests()
    {
        _handler = new GetOrdersByRestaurantIdHandler(_reader.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderRequests_FromReader()
    {
        var restaurantId = Guid.NewGuid();
        var dtos = new[]
        {
            new OrderRequestDto(Guid.NewGuid(), restaurantId, Guid.NewGuid(), OrderRequestStatus.Pending,
                DateTime.UtcNow)
        };
        var query = new GetOrdersByRestaurantIdQuery(restaurantId, null, 10, null);
        _reader.Setup(r => r.GetAllByRestaurantIdAsync(restaurantId, null, query.Limit, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dtos);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task Handle_ShouldPassCursorAndStatusFilter_ToReader()
    {
        var restaurantId = Guid.NewGuid();
        var cursor = Guid.NewGuid();
        var query = new GetOrdersByRestaurantIdQuery(restaurantId, cursor, 5, OrderRequestStatus.Approved);
        _reader.Setup(r => r.GetAllByRestaurantIdAsync(restaurantId, cursor, 5, OrderRequestStatus.Approved,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _handler.Handle(query, CancellationToken.None);

        _reader.Verify(r => r.GetAllByRestaurantIdAsync(restaurantId, cursor, 5, OrderRequestStatus.Approved,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenReaderFindsNothing()
    {
        var restaurantId = Guid.NewGuid();
        var query = new GetOrdersByRestaurantIdQuery(restaurantId, null, 10, null);
        _reader.Setup(r => r.GetAllByRestaurantIdAsync(restaurantId, null, query.Limit, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
