using OrderRequests.Application.Abstractions;

namespace OrderRequests.Infrastructure.Persistence;

public class UnitOfWork(OrderRequestsDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}