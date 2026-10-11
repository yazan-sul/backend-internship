using RestaurantReservation.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RestaurantReservation.Db.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", table => table.HasCheckConstraint("CK_TotalAmount", "[total_amount] >= 0"));
        builder.HasKey(order => order.OrderId);
        builder.Property(order => order.OrderId).HasColumnName("order_id").ValueGeneratedOnAdd();
        builder.Property(order => order.EmployeeId).HasColumnName("employee_id").IsRequired();
        builder.Property(order => order.ReservationId).HasColumnName("reservation_id").IsRequired();
        builder.Property(order => order.TotalAmount).HasColumnName("total_amount").IsRequired().HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(order => order.OrderDate).HasColumnName("order_date").HasColumnType("datetime2").IsRequired();

        builder.HasOne(order => order.Employee)
            .WithMany(employee => employee.Orders)
            .HasForeignKey(order => order.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(order => order.Reservation)
            .WithMany(reservation => reservation.Orders)
            .HasForeignKey(order => order.ReservationId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}