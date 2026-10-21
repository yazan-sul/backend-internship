using RestaurantReservation.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RestaurantReservation.Db.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations", table => table.HasCheckConstraint("CK_PartySize", "[party_size] > 0"));
        builder.HasKey(reservation => reservation.ReservationId);
        builder.Property(reservation => reservation.ReservationId).HasColumnName("reservation_id").ValueGeneratedOnAdd();
        builder.Property(reservation => reservation.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(reservation => reservation.RestaurantId).HasColumnName("restaurant_id").IsRequired();
        builder.Property(reservation => reservation.TableId).HasColumnName("table_id").IsRequired();
        builder.Property(reservation => reservation.ReservationDate).HasColumnName("reservation_date").IsRequired();
        builder.Property(reservation => reservation.PartySize).HasColumnName("party_size").IsRequired();

        builder.HasIndex(reservation => new { reservation.TableId, reservation.ReservationDate }).IsUnique();
        builder.HasOne(reservation => reservation.Customer)
            .WithMany(customer => customer.Reservations)
            .HasForeignKey(reservation => reservation.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(reservation => reservation.Restaurant)
            .WithMany(restaurant => restaurant.Reservations)
            .HasForeignKey(reservation => reservation.RestaurantId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(reservation => reservation.Table)
            .WithMany(restaurantTable => restaurantTable.Reservations)
            .HasForeignKey(reservation => new { reservation.TableId, reservation.RestaurantId })
            .HasPrincipalKey(restaurantTable => new { restaurantTable.TableId, restaurantTable.RestaurantId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
