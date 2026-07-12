using Microsoft.EntityFrameworkCore;
using Ordering.Application.Abstractions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;

namespace Ordering.Infrastructure.Persistence.Repositories;

public class OrderRepository(OrderingDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await context.Orders.Where(o => o.Id == id).Include(o => o.OrderLines)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByMenuItemIdAsync(MenuItemRefId menuItemRefId,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders.Where(o => o.OrderLines.Any(l => l.MenuItemRefId == menuItemRefId))
            .Where(o => o.Status == OrderStatus.Draft)
            .Include(o => o.OrderLines)
            .ToListAsync(cancellationToken);
    }

    public void Add(Order order)
    {
        context.Orders.Add(order);
    }
}