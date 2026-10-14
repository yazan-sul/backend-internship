using RestaurantReservation.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RestaurantReservation.Db.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems", table => table.HasCheckConstraint("CK_Price", "[price] >= 0"));
        builder.HasKey(menuItem => menuItem.MenuItemId);
        builder.Property(menuItem => menuItem.MenuItemId).HasColumnName("menu_item_id").ValueGeneratedOnAdd();
        builder.Property(menuItem => menuItem.RestaurantId).HasColumnName("restaurant_id").IsRequired();
        builder.Property(menuItem => menuItem.Name).HasColumnName("name").IsRequired().HasMaxLength(120);
        builder.Property(menuItem => menuItem.Description).HasColumnName("description").IsRequired(false).HasMaxLength(1000);
        builder.Property(menuItem => menuItem.Price).HasColumnName("price").IsRequired().HasPrecision(18, 2);

        builder.HasOne(menuItem => menuItem.Restaurant)
            .WithMany(restaurant => restaurant.MenuItems)
            .HasForeignKey(menuItem => menuItem.RestaurantId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
