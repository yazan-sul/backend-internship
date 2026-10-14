using RestaurantReservation.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RestaurantReservation.Db.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");
        builder.HasKey(restaurant => restaurant.RestaurantId);
        builder.Property(restaurant => restaurant.RestaurantId).HasColumnName("restaurant_id").ValueGeneratedOnAdd();
        builder.Property(restaurant => restaurant.Name).HasColumnName("name").IsRequired().HasMaxLength(120);
        builder.Property(restaurant => restaurant.Address).HasColumnName("address").IsRequired().HasMaxLength(250);
        builder.Property(restaurant => restaurant.PhoneNumber).HasColumnName("phone_number").IsRequired().HasMaxLength(30);
        builder.Property(restaurant => restaurant.OpeningHours).HasColumnName("opening_hours").IsRequired().HasMaxLength(200);
    }
}