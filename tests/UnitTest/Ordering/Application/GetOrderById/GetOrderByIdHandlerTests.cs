using FluentAssertions;
using Moq;
using Ordering.Application.Abstractions;
using Ordering.Application.GetOrderById;
using Ordering.Domain.Enums;

namespace Ordering.UnitTest.Application.GetOrderById;

public class GetOrderByIdHandlerTests
{
    private Mock<IOrderReader> _reader = new();

    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _handler = new GetOrderByIdHandler(_reader.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderDto_WhenFound()
    {
        var orderId = Guid.NewGuid();
        var dto = new OrderDto(orderId, OrderStatus.Draft, Guid.NewGuid(), null, [], DateTime.UtcNow);
        var query = new GetOrderByIdQuery(orderId);
        _reader.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(dto);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotFound()
    {
        var orderId = Guid.NewGuid();
        var query = new GetOrderByIdQuery(orderId);
        _reader.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((OrderDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}