using RestaurantReservation.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RestaurantReservation.Db.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(employee => employee.EmployeeId);
        builder.Property(employee => employee.EmployeeId).HasColumnName("employee_id").ValueGeneratedOnAdd();
        builder.Property(employee => employee.RestaurantId).HasColumnName("restaurant_id").IsRequired();
        builder.Property(employee => employee.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(80);
        builder.Property(employee => employee.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(80);
        builder.Property(employee => employee.Position).HasColumnName("position").IsRequired().HasMaxLength(50);

        builder.HasOne(employee => employee.Restaurant)
            .WithMany(restaurant => restaurant.Employees)
            .HasForeignKey(employee => employee.RestaurantId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
