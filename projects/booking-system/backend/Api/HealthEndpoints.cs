using Npgsql;

namespace AirportTicketBookingSystem.Api;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/health", async (NpgsqlDataSource dataSource, CancellationToken ct) =>
        {
            try
            {
                await using var command = dataSource.CreateCommand("SELECT 1");
                await command.ExecuteScalarAsync(ct);

                return Results.Ok(new
                {
                    message = "API and database are ready",
                    services = new { backend = "healthy", postgresql = "healthy" }
                });
            }
            catch (NpgsqlException)
            {
                return Results.Json(
                    new
                    {
                        message = "API is ready, but the database is unavailable",
                        services = new { backend = "healthy", postgresql = "unavailable" }
                    },
                    statusCode: 503);
            }
        });

        return endpoints;
    }
}
