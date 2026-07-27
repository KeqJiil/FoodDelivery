using MassTransit;
using Microsoft.EntityFrameworkCore;
using Saga.Application;

namespace Saga.Infrastructure.Persistence;

public class SagaDbContext : DbContext
{
    public DbSet<OrderState> OrderStates { get; set; }

    public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<OrderState>();

        builder.ToTable("order_states").HasKey(x => x.CorrelationId);
        builder.Property(x => x.CorrelationId).HasColumnName("correlation_id");
        builder.Property(x => x.Amount).HasColumnName("amount");
        builder.Property(x => x.FailedAt).HasColumnName("failed_at");
        builder.Property(x => x.CurrentState).HasColumnName("current_state");
        builder.Property(x => x.ApprovalTimeoutTokenId).HasColumnName("approval_timeout_token_id");
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Property(x => x.Currency).HasColumnName("currency");
        builder.Property(x => x.PaymentTimeoutTokenId).HasColumnName("payment_timeout_token_id");
        builder.Property(x => x.PaymentId).HasColumnName("payment_id");

        builder.HasIndex(x => x.PaymentId).IsUnique();

        modelBuilder.HasDefaultSchema("saga");

        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}