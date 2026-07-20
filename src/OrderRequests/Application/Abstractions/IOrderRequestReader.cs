using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Domain.Enums;

namespace OrderRequests.Application.Abstractions;

public interface IOrderRequestReader
{
    public Task<OrderRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    public Task<IEnumerable<OrderRequestDto>> GetAllByRestaurantIdAsync(Guid restaurantId, Guid? cursor, byte limit,
        OrderRequestStatus? statusFilter, CancellationToken ct = default);
}