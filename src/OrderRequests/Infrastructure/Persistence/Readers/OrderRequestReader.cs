using Microsoft.EntityFrameworkCore;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Ids;

namespace OrderRequests.Infrastructure.Persistence.Readers;

public class OrderRequestReader(OrderRequestsDbContext context) : IOrderRequestReader
{
    public async Task<OrderRequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orderRequestId = new OrderRequestId(id);
        return await context.OrderRequests.AsNoTracking().Where(x => x.Id == orderRequestId).Select(x =>
            new OrderRequestDto(x.Id.Id, x.RestaurantRefId.Id, x.OrderRefId.Id, x.Status,
                EF.Property<DateTime>(x, "CreatedAt"))).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderRequestDto>> GetAllByRestaurantIdAsync(Guid restaurantId,
        DateTime? cursorCreatedAt, Guid? cursorId, byte limit, OrderRequestStatus? statusFilter,
        CancellationToken ct = default)
    {
        var restaurantRefId = new RestaurantRefId(restaurantId);

        var query = context.OrderRequests.AsNoTracking()
            .Where(x => x.RestaurantRefId == restaurantRefId);

        if (statusFilter.HasValue)
            query = query.Where(x => x.Status == statusFilter.Value);

        if (cursorCreatedAt.HasValue && cursorId.HasValue)
        {
            var cursorOrderRequestId = new OrderRequestId(cursorId.Value);
            query = query.Where(x =>
                EF.Property<DateTime>(x, "CreatedAt") > cursorCreatedAt.Value ||
                (EF.Property<DateTime>(x, "CreatedAt") == cursorCreatedAt.Value &&
                 x.Id.CompareTo(cursorOrderRequestId) > 0));
        }

        return await query
            .OrderBy(x => EF.Property<DateTime>(x, "CreatedAt"))
            .ThenBy(x => x.Id)
            .Take(limit)
            .Select(x => new OrderRequestDto(x.Id.Id, x.RestaurantRefId.Id, x.OrderRefId.Id, x.Status,
                EF.Property<DateTime>(x, "CreatedAt")))
            .ToListAsync(ct);
    }
}