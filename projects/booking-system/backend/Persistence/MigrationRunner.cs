using System.Reflection;
using Npgsql;

namespace AirportTicketBookingSystem.Persistence;

public sealed class MigrationRunner(NpgsqlDataSource dataSource)
{
    private const string ResourcePrefix =
        "AirportTicketBookingSystem.Persistence.Migrations.";

    public async Task ApplyAsync(CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        await using (var command = new NpgsqlCommand("""
            CREATE TABLE IF NOT EXISTS schema_migrations (
                migration_name varchar(255) PRIMARY KEY,
                applied_at timestamptz NOT NULL
            )
            """, connection))
        {
            await command.ExecuteNonQueryAsync(ct);
        }

        var appliedMigrations = await GetAppliedMigrationsAsync(connection, ct);
        var migrations = GetMigrationResources();

        foreach (var migration in migrations)
        {
            if (appliedMigrations.Contains(migration.Name))
            {
                continue;
            }

            await ApplyMigrationAsync(connection, migration, ct);
        }
    }

    private static async Task<HashSet<string>> GetAppliedMigrationsAsync(
        NpgsqlConnection connection,
        CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            "SELECT migration_name FROM schema_migrations",
            connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var applied = new HashSet<string>(StringComparer.Ordinal);

        while (await reader.ReadAsync(ct))
        {
            applied.Add(reader.GetString(0));
        }

        return applied;
    }

    private static async Task ApplyMigrationAsync(
        NpgsqlConnection connection,
        MigrationResource migration,
        CancellationToken ct)
    {
        await using var stream = migration.Assembly.GetManifestResourceStream(migration.ResourceName)
            ?? throw new InvalidOperationException($"Migration resource not found: {migration.ResourceName}");
        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync(ct);

        await using var transaction = await connection.BeginTransactionAsync(ct);

        await using (var command = new NpgsqlCommand(sql, connection, transaction))
        {
            await command.ExecuteNonQueryAsync(ct);
        }

        await using (var command = new NpgsqlCommand(
            "INSERT INTO schema_migrations (migration_name, applied_at) VALUES ($1, CURRENT_TIMESTAMP)",
            connection,
            transaction))
        {
            command.Parameters.AddWithValue(migration.Name);
            await command.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    private static IReadOnlyList<MigrationResource> GetMigrationResources()
    {
        var assembly = typeof(MigrationRunner).Assembly;

        return assembly
            .GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .Select(name => new MigrationResource(
                name[ResourcePrefix.Length..],
                name,
                assembly))
            .OrderBy(migration => migration.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private sealed record MigrationResource(
        string Name,
        string ResourceName,
        Assembly Assembly);
}
