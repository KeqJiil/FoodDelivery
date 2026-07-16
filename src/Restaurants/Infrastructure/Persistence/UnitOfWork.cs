using Restaurants.Application.Abstractions;

namespace Restaurants.Infrastructure.Persistence;

public class UnitOfWork(RestaurantsDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}