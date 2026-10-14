using RestaurantReservation.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RestaurantReservation.Db.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", table => table.HasCheckConstraint("CK_Quantity", "[quantity] > 0"));

        builder.HasKey(orderItem => orderItem.OrderItemId);

        builder.Property(orderItem => orderItem.OrderItemId).HasColumnName("order_item_id").ValueGeneratedOnAdd();

        builder.Property(orderItem => orderItem.OrderId).HasColumnName("order_id").IsRequired();

        builder.Property(orderItem => orderItem.ItemId).HasColumnName("item_id").IsRequired();

        builder.Property(orderItem => orderItem.Quantity).HasColumnName("quantity").IsRequired();

        builder.HasOne(orderItem => orderItem.Order)
            .WithMany(order => order.OrderItems)
            .HasForeignKey(orderItem => orderItem.OrderId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(orderItem => orderItem.MenuItem)
            .WithMany(menuItem => menuItem.OrderItems)
            .HasForeignKey(orderItem => orderItem.ItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}