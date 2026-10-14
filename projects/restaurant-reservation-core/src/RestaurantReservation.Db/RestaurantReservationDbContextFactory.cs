using Microsoft.EntityFrameworkCore.Design;

namespace RestaurantReservation.Db;

public class RestaurantReservationDbContextFactory
    : IDesignTimeDbContextFactory<RestaurantReservationDbContext>
{
    public RestaurantReservationDbContext CreateDbContext(string[] args)
    {
        return new RestaurantReservationDbContext(DatabaseConfiguration.CreateOptions());
    }
}
