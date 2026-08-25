using Npgsql;

namespace InventorySystem.Migrations;

/// <summary>
/// Applies the database migrations required by the inventory service at startup.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly string connectionString;

    /// <summary>
    /// Creates an initializer using the configured database connection string.
    /// </summary>
    /// <param name="configuration">Application configuration containing the database connection.</param>
    /// <exception cref="InvalidOperationException">Thrown when the connection string is missing.</exception>
    public DatabaseInitializer(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is required.");
    }

    /// <summary>
    /// Executes the checked-in SQL migration files in their required order.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel database startup work.</param>
    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var migrationPath = Path.Combine(AppContext.BaseDirectory, "Migrations", "001_create_products.sql");
        var migrationSql = await File.ReadAllTextAsync(migrationPath, cancellationToken);

        await using var command = new NpgsqlCommand(migrationSql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
