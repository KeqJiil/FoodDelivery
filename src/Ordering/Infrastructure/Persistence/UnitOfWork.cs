using Microsoft.EntityFrameworkCore;
using Ordering.Application.Abstractions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Entities;

namespace Ordering.Infrastructure.Persistence;

public class UnitOfWork(DbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public void ClearChangeTracker()
    {
        var entries = context.ChangeTracker.Entries<Order>()
            .Cast<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry>()
            .Concat(context.ChangeTracker.Entries<OrderLine>())
            .ToList();

        foreach (var entry in entries)
            entry.State = EntityState.Detached;
    }
}