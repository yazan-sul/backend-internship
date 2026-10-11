using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db.Models;
using RestaurantReservation.Db.Configurations;
namespace RestaurantReservation.Db;

public class RestaurantReservationDbContext(
    DbContextOptions<RestaurantReservationDbContext> options) : DbContext(options)
{
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<RestaurantTable> Tables => Set<RestaurantTable>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    override protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RestaurantConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new RestaurantTableConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new MenuItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
    }


}
