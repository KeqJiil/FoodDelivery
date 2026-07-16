using MassTransit;
using Microsoft.EntityFrameworkCore;
using Restaurants.Domain.Aggregates;

namespace Restaurants.Infrastructure.Persistence;

public class RestaurantsDbContext : DbContext
{
    public DbSet<Restaurant> Restaurants { get; set; }

    public RestaurantsDbContext(DbContextOptions<RestaurantsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RestaurantsDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}