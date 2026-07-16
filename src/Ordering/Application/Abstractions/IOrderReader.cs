using Ordering.Domain.Ids;
using Ordering.Application.GetOrderById;

namespace Ordering.Application.Abstractions;

public interface IOrderReader
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}