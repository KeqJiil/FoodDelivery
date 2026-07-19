using Microsoft.EntityFrameworkCore;
using OrderRequests.Application.Abstractions;
using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Ids;

namespace OrderRequests.Infrastructure.Persistence.Repositories;

public class OrderRequestRepository(OrderRequestsDbContext ctx) : IOrderRequestRepository
{
    public async Task<OrderRequest?> GetByIdAsync(OrderRequestId id, CancellationToken cancellationToken)
    {
        return await ctx.OrderRequests.Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(OrderRequest orderRequest)
    {
        ctx.Add(orderRequest);
    }
}