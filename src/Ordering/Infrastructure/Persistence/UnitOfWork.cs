using Ordering.Application.Abstractions;

namespace Ordering.Infrastructure.Persistence;

public class UnitOfWork(OrderingDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}