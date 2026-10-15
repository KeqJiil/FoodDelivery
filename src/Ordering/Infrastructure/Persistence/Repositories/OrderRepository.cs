using Microsoft.EntityFrameworkCore;
using Ordering.Application.Abstractions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;

namespace Ordering.Infrastructure.Persistence.Repositories;

public class OrderRepository(DbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await context.Set<Order>().Where(o => o.Id == id).Include(o => o.OrderLines)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByIdsAsync(IReadOnlyCollection<OrderId> ids,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Order>().Where(o => ids.Contains(o.Id))
            .Include(o => o.OrderLines)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderId>> GetDraftIdsByMenuItemIdAsync(MenuItemRefId menuItemRefId, int limit,
        OrderId? afterId = null, CancellationToken cancellationToken = default)
    {
        var query = context.Set<Order>().Where(o => o.OrderLines.Any(l => l.MenuItemRefId == menuItemRefId))
            .Where(o => o.Status == OrderStatus.Draft);

        if (afterId is not null)
            query = query.Where(o => o.Id > afterId);

        return await query.OrderBy(o => o.Id)
            .Select(o => o.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public void Add(Order order)
    {
        context.Set<Order>().Add(order);
    }
}