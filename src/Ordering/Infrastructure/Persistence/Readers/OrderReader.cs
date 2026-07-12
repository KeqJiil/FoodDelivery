using Microsoft.EntityFrameworkCore;
using Ordering.Application.Abstractions;
using Ordering.Application.GetOrderById;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Persistence.Configurations;

namespace Ordering.Infrastructure.Persistence.Readers;

public class OrderReader(OrderingDbContext context) : IOrderReader
{
    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orderId = new OrderId(id);
        var raw = await context.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => new RawOrderDto(
            o.Id.Id,
            o.Status,
            o.RestaurantRefId.Id,
            o.OrderLines.Select(l => new OrderLineDto(l.Id.Id, l.Price, l.Quantity, l.MenuItemRefId.Id)).ToList(),
            EF.Property<DateTime>(o, OrderShadowProperties.CreatedAt)
        )).FirstOrDefaultAsync(cancellationToken);
        if (raw is null) return null;

        var totalPrice = raw.OrderLines.Count == 0
            ? null
            : raw.OrderLines.Select(l => l.Price * l.Quantity).Aggregate((a, b) => a + b);

        return new OrderDto(raw.Id, raw.Status, raw.RestaurantRefId, totalPrice, raw.OrderLines, raw.CreatedAt);
    }
    
    private record RawOrderDto(
        Guid Id,
        OrderStatus Status,
        Guid RestaurantRefId,
        List<OrderLineDto> OrderLines,
        DateTime CreatedAt);
}