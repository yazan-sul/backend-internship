using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("ConnectionStrings:Database is required.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));

var app = builder.Build();

app.MapGet("/api/health", async (NpgsqlDataSource dataSource, CancellationToken cancellationToken) =>
{
    try
    {
        await using var command = dataSource.CreateCommand("SELECT 1;");
        await command.ExecuteScalarAsync(cancellationToken);

        return Results.Ok(new
        {
            message = "API and database are ready",
            services = new { backend = "healthy", postgresql = "healthy" }
        });
    }
    catch (NpgsqlException)
    {
        return Results.Json(new
        {
            message = "API is ready, but the database is unavailable",
            services = new { backend = "healthy", postgresql = "unavailable" }
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();
