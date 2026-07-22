using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Aggregates;
using Payments.Domain.Ids;

namespace Payments.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments").HasKey(p => p.Id);

        builder.HasIndex(p => p.OrderRefId).IsUnique().HasDatabaseName("idx_payment_order_id");

        builder.Property(p => p.Id).HasColumnName("id").HasConversion(p => p.Id, p => new PaymentId(p));
        builder.Property(p => p.OrderRefId).HasColumnName("order_id")
            .HasConversion(p => p.Id, p => new OrderRefId(p));
        builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(p => p.FailureReason).HasColumnName("failure_reason");

        builder.Property<DateTime>("CreatedAt").HasColumnName("created_at").HasDefaultValueSql("NOW()");

        builder.OwnsOne(p => p.Amount, a =>
        {
            a.Property(m => m.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            a.Property(m => m.Currency).HasColumnName("currency").HasConversion<string>().IsRequired();
        });

        builder.Ignore(p => p.Events);
    }
}
