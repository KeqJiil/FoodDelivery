using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Ids;

namespace OrderRequests.Infrastructure.Persistence.Configurations;

public class OrderRequestConfiguration : IEntityTypeConfiguration<OrderRequest>
{
    public void Configure(EntityTypeBuilder<OrderRequest> builder)
    {
        builder.ToTable("order_requests").HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("id").HasConversion(o => o.Id, o => new OrderRequestId(o));
        builder.Property(o => o.OrderRefId).HasColumnName("order_id").HasConversion(o => o.Id, o => new OrderRefId(o));
        builder.Property(o => o.RestaurantRefId).HasColumnName("restaurant_id")
            .HasConversion(o => o.Id, o => new RestaurantRefId(o));
        builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>().IsRequired();

        builder.Property<DateTime>("CreatedAt").HasColumnName("created_at").HasDefaultValueSql("NOW()");

        builder.Ignore(o => o.Events);
    }
}