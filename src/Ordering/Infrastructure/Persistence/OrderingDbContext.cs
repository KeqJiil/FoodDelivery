using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Aggregates;

namespace Ordering.Infrastructure.Persistence;

public class OrderingDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}