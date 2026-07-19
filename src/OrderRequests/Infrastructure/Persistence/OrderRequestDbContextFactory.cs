using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderRequests.Infrastructure.Persistence;

public sealed class OrderRequestDbContextFactory : IDesignTimeDbContextFactory<OrderRequestsDbContext>
{
    public OrderRequestsDbContext CreateDbContext(string[] args)
    {
        var connectionStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (connectionStr is null)
            throw new InvalidOperationException(
                "No connection string found.");

        var optionBuilder = new DbContextOptionsBuilder<OrderRequestsDbContext>();

        optionBuilder.UseNpgsql(connectionStr);

        return new OrderRequestsDbContext(optionBuilder.Options);
    }
}