using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Restaurants.Infrastructure.Persistence;

public sealed class RestaurantsDbContextFactory : IDesignTimeDbContextFactory<RestaurantsDbContext>
{
    public RestaurantsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (connectionString is null)
            throw new InvalidOperationException(
                "No connection string found.");

        var optionsBuilder = new DbContextOptionsBuilder<RestaurantsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new RestaurantsDbContext(optionsBuilder.Options);
    }
}