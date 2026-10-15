using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Saga.Application;

namespace Saga.Infrastructure.Persistence.Configurations;

public class OrderStateConfiguration : IEntityTypeConfiguration<OrderState>
{
    public void Configure(EntityTypeBuilder<OrderState> builder)
    {
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
    }
}
