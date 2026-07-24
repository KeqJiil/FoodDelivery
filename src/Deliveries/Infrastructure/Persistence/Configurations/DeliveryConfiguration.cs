using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deliveries.Infrastructure.Persistence.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("deliveries").HasKey(d => d.Id);

        builder.HasIndex(d => d.OrderRefId).IsUnique().HasDatabaseName("idx_delivery_order_id");

        builder.Property(d => d.Id).HasColumnName("id").HasConversion(d => d.Id, d => new DeliveryId(d));
        builder.Property(d => d.OrderRefId).HasColumnName("order_id")
            .HasConversion(d => d.Id, d => new OrderRefId(d));
        builder.Property(d => d.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(d => d.FailureReason).HasColumnName("failure_reason");

        builder.Property<DateTime>("CreatedAt").HasColumnName("created_at").HasDefaultValueSql("NOW()");

        builder.Ignore(d => d.Events);
    }
}
