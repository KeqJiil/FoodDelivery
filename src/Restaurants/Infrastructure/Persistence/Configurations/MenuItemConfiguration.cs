using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurants.Domain.Entities;
using Restaurants.Domain.Ids;

namespace Restaurants.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items").HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id").HasConversion(x => x.Id, x => new MenuItemId(x));

        builder.OwnsOne(p => p.Name,
            n => { n.Property(x => x.Data).HasColumnName("name").HasMaxLength(30).IsRequired(); });

        builder.OwnsOne(p => p.Description,
            d => { d.Property(x => x.Data).HasColumnName("description").HasMaxLength(200).IsRequired(); });

        builder.OwnsOne(p => p.Price, p =>
        {
            p.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            p.Property(x => x.Currency).HasColumnName("currency").HasConversion<string>().IsRequired();
        });
    }
}