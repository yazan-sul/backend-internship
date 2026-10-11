using Microsoft.EntityFrameworkCore;

namespace RestaurantReservation.Db;

public class RestaurantReservationDbContext(
    DbContextOptions<RestaurantReservationDbContext> options) : DbContext(options)
{
    // Phase 5: add your DbSet properties and entity mappings here.
}
