using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Saga.Infrastructure.Persistence;

public class SagaDbContextFactory : IDesignTimeDbContextFactory<SagaDbContext>
{
    public SagaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (connectionString is null)
            throw new InvalidOperationException(
                "No connection string found.");

        var optionsBuilder = new DbContextOptionsBuilder<SagaDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new SagaDbContext(optionsBuilder.Options);
    }
}