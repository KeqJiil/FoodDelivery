using Microsoft.EntityFrameworkCore;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Domain.Enums;

namespace OrderRequests.Infrastructure.Persistence.Readers;

public class OrderRequestReader(OrderRequestsDbContext context) : IOrderRequestReader
{
    public async Task<OrderRequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.OrderRequests.AsNoTracking().Where(x => x.Id.Id == id).Select(x =>
            new OrderRequestDto(x.Id.Id, x.RestaurantRefId.Id, x.OrderRefId.Id, x.Status,
                EF.Property<DateTime>(x, "CreatedAt"))).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderRequestDto>> GetAllByRestaurantIdAsync(Guid restaurantId, Guid? cursor,
        byte limit, OrderRequestStatus? statusFilter,
        CancellationToken ct = default)
    {
        return await context.OrderRequests.AsNoTracking()
            .Where(x => x.RestaurantRefId.Id == restaurantId)
            .Where(x => statusFilter == null || x.Status == statusFilter)
            .Where(x => cursor == null || x.Id.Id > cursor.Value)
            .OrderBy(x => x.Id.Id)
            .Take(limit)
            .Select(x =>
                new OrderRequestDto(x.Id.Id, x.RestaurantRefId.Id, x.OrderRefId.Id, x.Status,
                    EF.Property<DateTime>(x, "CreatedAt")))
            .ToListAsync(ct);
    }
}