using OrderRequests.Application.GetOrderRequestById;

namespace OrderRequests.Application.Abstractions;

public interface IOrderRequestReader
{
    public Task<OrderRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
}