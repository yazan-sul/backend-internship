using Microsoft.EntityFrameworkCore;

namespace RestaurantReservation.Db;

public static class DatabaseConfiguration
{
    public const string ConnectionStringVariable = "ConnectionStrings__RestaurantReservationCore";

    public static DbContextOptions<RestaurantReservationDbContext> CreateOptions()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Set {ConnectionStringVariable} before using database commands. See docs/setup.md.");
        }

        return new DbContextOptionsBuilder<RestaurantReservationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
    }
}
