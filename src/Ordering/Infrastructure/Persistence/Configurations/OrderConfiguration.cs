using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;

namespace Ordering.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders").HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("id").HasConversion(o => o.Id, o => new OrderId(o));
        builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(o => o.RestaurantRefId).HasColumnName("restaurant_ref_id")
            .HasConversion(o => o.Id, o => new RestaurantRefId(o));
        builder.Property<DateTime>(OrderShadowProperties.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.HasMany(o => o.OrderLines).WithOne().HasForeignKey("order_id");
        builder.Navigation(o => o.OrderLines).HasField("_orderLines").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(o => o.TotalPrice);
        builder.Ignore(o => o.Events);
    }
}