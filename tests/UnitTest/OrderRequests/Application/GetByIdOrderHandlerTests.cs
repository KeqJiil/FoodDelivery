using FluentAssertions;
using Moq;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Domain.Enums;

namespace OrderRequests.UnitTest.Application;

public class GetByIdOrderHandlerTests
{
    private Mock<IOrderRequestReader> _reader = new();

    private readonly GetOrderRequestByIdHandler _handler;

    public GetByIdOrderHandlerTests()
    {
        _handler = new GetOrderRequestByIdHandler(_reader.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderRequestDto_WhenFound()
    {
        var id = Guid.NewGuid();
        var dto = new OrderRequestDto(id, Guid.NewGuid(), Guid.NewGuid(), OrderRequestStatus.Pending,
            DateTime.UtcNow);
        var query = new GetOrderRequestByIdQuery(id);
        _reader.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(dto);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var query = new GetOrderRequestByIdQuery(id);
        _reader.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((OrderRequestDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
