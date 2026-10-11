using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db.Models;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RestaurantReservation.Db.Configurations;

public class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
{
    public void Configure(EntityTypeBuilder<RestaurantTable> builder)
    {
        builder.ToTable("RestaurantTables", table => table.HasCheckConstraint("CK_Capacity", "[capacity] > 0"));
        builder.HasKey(restaurantTable => restaurantTable.TableId);
        builder.Property(restaurantTable => restaurantTable.TableId).HasColumnName("table_id").ValueGeneratedOnAdd();
        builder.Property(restaurantTable => restaurantTable.RestaurantId).HasColumnName("restaurant_id").IsRequired();
        builder.Property(restaurantTable => restaurantTable.Capacity).HasColumnName("capacity").IsRequired();
        
        builder.HasOne(restaurantTable => restaurantTable.Restaurant)
            .WithMany(restaurant => restaurant.Tables)
            .HasForeignKey(restaurantTable => restaurantTable.RestaurantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasAlternateKey(restaurantTable => new { restaurantTable.TableId, restaurantTable.RestaurantId });

    }
}