using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;

namespace Ordering.Application.Abstractions;

public interface IOrderRepository
{
    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Order>> GetByIdsAsync(IReadOnlyCollection<OrderId> ids,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<OrderId>> GetDraftIdsByMenuItemIdAsync(MenuItemRefId menuItemRefId, int limit,
        OrderId? afterId = null, CancellationToken cancellationToken = default);

    public void Add(Order order);
}
