using RestaurantReservation.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RestaurantReservation.Db.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.CustomerId);
        builder.Property(customer => customer.CustomerId).HasColumnName("customer_id").ValueGeneratedOnAdd();
        builder.Property(customer => customer.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(80);
        builder.Property(customer => customer.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(80);
        builder.Property(customer => customer.PhoneNumber).HasColumnName("phone_number").IsRequired().HasMaxLength(30);
        builder.Property(customer => customer.Email).HasColumnName("email").IsRequired().HasMaxLength(254).UseCollation("Latin1_General_100_CI_AS");
        builder.HasIndex(customer => customer.Email).IsUnique();
    }
}