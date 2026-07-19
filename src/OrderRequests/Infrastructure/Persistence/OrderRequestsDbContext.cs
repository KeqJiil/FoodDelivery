using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderRequests.Domain.Aggregates;

namespace OrderRequests.Infrastructure.Persistence;

public class OrderRequestsDbContext : DbContext
{
    public DbSet<OrderRequest> OrderRequests { get; set; }

    public OrderRequestsDbContext(DbContextOptions<OrderRequestsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderRequestsDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}