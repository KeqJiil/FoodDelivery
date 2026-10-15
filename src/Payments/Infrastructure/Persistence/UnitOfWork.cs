using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;

namespace Payments.Infrastructure.Persistence;

public class UnitOfWork(DbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
