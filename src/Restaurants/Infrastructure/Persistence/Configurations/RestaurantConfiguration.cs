using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Ids;

namespace Restaurants.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("restaurants").HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Id, x => new RestaurantId(x));
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().IsRequired();

        builder.OwnsOne(x => x.Name,
            n => { n.Property(p => p.Data).HasColumnName("name").HasMaxLength(30).IsRequired(); });

        builder.OwnsOne(x => x.Description,
            d => { d.Property(p => p.Data).HasColumnName("description").HasMaxLength(200).IsRequired(); });

        builder.OwnsOne(x => x.MinimalOrderPrice, p =>
        {
            p.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            p.Property(x => x.Currency).HasColumnName("currency").HasConversion<string>().IsRequired();
        });

        builder.OwnsOne(x => x.Schedule, s => { s.OwnsMany(sch => sch.OpeningWindows); });

        builder.HasMany(x => x.MenuItems).WithOne().HasForeignKey("restaurant_id");
        builder.Navigation(x => x.MenuItems).HasField("_menuItems").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.Events);
    }
}