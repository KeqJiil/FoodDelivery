using Deliveries.Domain.Aggregates;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Deliveries.Infrastructure.Persistence;

public class DeliveriesDbContext : DbContext
{
    public DbSet<Delivery> Deliveries { get; set; }

    public DeliveriesDbContext(DbContextOptions<DeliveriesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveriesDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
