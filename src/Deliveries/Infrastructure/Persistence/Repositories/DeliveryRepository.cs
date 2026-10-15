using Deliveries.Application.Abstractions;
using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Ids;
using Microsoft.EntityFrameworkCore;

namespace Deliveries.Infrastructure.Persistence.Repositories;

public class DeliveryRepository(DbContext ctx) : IDeliveryRepository
{
    public async Task<Delivery?> GetByIdAsync(DeliveryId id, CancellationToken cancellationToken = default)
    {
        return await ctx.Set<Delivery>().Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Delivery delivery)
    {
        ctx.Add(delivery);
    }
}
