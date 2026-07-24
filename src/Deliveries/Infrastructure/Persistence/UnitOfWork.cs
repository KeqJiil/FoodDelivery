using Deliveries.Application.Abstractions;

namespace Deliveries.Infrastructure.Persistence;

public class UnitOfWork(DeliveriesDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
