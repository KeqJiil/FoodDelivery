using Microsoft.EntityFrameworkCore;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.GetOrderRequestById;

namespace OrderRequests.Infrastructure.Persistence.Readers;

public class OrderRequestReader(OrderRequestsDbContext context) : IOrderRequestReader
{
    public async Task<OrderRequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.OrderRequests.AsNoTracking().Where(x => x.Id.Id == id).Select(x =>
            new OrderRequestDto(x.Id.Id, x.RestaurantRefId.Id, x.OrderRefId.Id, x.Status,
                EF.Property<DateTime>(x, "CreatedAt"))).FirstOrDefaultAsync(cancellationToken);
    }
}