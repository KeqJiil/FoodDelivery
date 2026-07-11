using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Entities;
using Ordering.Domain.Ids;

namespace Ordering.Infrastructure.Persistence.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines").HasKey(o => o.Id);

        builder.HasIndex(o => o.MenuItemRefId).HasDatabaseName("menu_ref_index");

        builder.Property(o => o.Id).HasColumnName("id").HasConversion(o => o.Id, o => new OrderLineId(o));
        builder.Property(o => o.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(o => o.MenuItemRefId).HasColumnName("menu_item_ref_id")
            .HasConversion(o => o.Id, o => new MenuItemRefId(o));

        builder.OwnsOne(o => o.Price, p =>
        {
            p.Property(m => m.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            p.Property(m => m.Currency).HasColumnName("currency").HasConversion<string>().IsRequired();
        });
    }
}