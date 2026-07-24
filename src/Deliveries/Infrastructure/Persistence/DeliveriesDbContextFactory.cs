using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Deliveries.Infrastructure.Persistence;

public sealed class DeliveriesDbContextFactory : IDesignTimeDbContextFactory<DeliveriesDbContext>
{
    public DeliveriesDbContext CreateDbContext(string[] args)
    {
        var connectionStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (connectionStr is null)
            throw new InvalidOperationException(
                "No connection string found.");

        var optionBuilder = new DbContextOptionsBuilder<DeliveriesDbContext>();

        optionBuilder.UseNpgsql(connectionStr);

        return new DeliveriesDbContext(optionBuilder.Options);
    }
}
