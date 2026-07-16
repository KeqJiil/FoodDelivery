using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;

namespace Ordering.Application.Abstractions;

public interface IOrderRepository
{
    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Order>> GetByMenuItemIdAsync(MenuItemRefId menuItemRefId,
        CancellationToken cancellationToken = default);

    public void Add(Order order);
}