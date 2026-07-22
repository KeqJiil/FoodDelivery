using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Aggregates;

namespace Payments.Infrastructure.Persistence;

public class PaymentsDbContext : DbContext
{
    public DbSet<Payment> Payments { get; set; }

    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
