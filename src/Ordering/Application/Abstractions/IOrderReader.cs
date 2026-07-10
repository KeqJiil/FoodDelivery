using Ordering.Application.GetOrderById;
using Ordering.Domain.Ids;

namespace Ordering.Application.Abstractions;

public interface IOrderReader
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}