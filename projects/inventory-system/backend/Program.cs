using Npgsql;
using InventorySystem.Endpoints;
using InventorySystem.Migrations;

// Configure the application and its shared PostgreSQL connection pool.
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("ConnectionStrings:Database is required.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<ProductRepository>();

var app = builder.Build();

// Apply local schema migrations before accepting requests.
await app.Services.GetRequiredService<DatabaseInitializer>().ApplyAsync();

// Keep this endpoint lightweight so it can be used by Docker and local development checks.
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

app.MapProductEndpoints();

app.Run();
