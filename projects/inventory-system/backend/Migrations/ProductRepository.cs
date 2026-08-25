using Npgsql;
using InventorySystem.Models;

namespace InventorySystem.Migrations;

/// <summary>
/// Encapsulates SQL access for products.
/// </summary>
public sealed class ProductRepository
{
    private readonly NpgsqlDataSource dataSource;

    /// <summary>
    /// Creates a product repository backed by the shared PostgreSQL data source.
    /// </summary>
    /// <param name="dataSource">The application PostgreSQL connection pool.</param>
    public ProductRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    /// <summary>
    /// Loads all products from the database.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the database operation.</param>
    /// <returns>A read-only list of products.</returns>
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, name, price, quantity, created_at, updated_at
            FROM products
            ORDER BY id;
            """;

        var products = new List<Product>();
        await using var command = dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(ReadProduct(reader));
        }

        return products;
    }

    /// <summary>
    /// Loads one product by its database identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the database operation.</param>
    /// <returns>The matching product, or <see langword="null"/> when it does not exist.</returns>
    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, name, price, quantity, created_at, updated_at
            FROM products
            WHERE id = $1;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? ReadProduct(reader)
            : null;
    }

    /// <summary>
    /// Updates an existing product and returns its persisted values.
    /// </summary>
    public async Task<Product?> UpdateAsync(
        int id,
        string name,
        decimal price,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE products
            SET name = $2, price = $3, quantity = $4, updated_at = CURRENT_TIMESTAMP
            WHERE id = $1
            RETURNING id, name, price, quantity, created_at, updated_at;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(id);
        command.Parameters.AddWithValue(name);
        command.Parameters.AddWithValue(price);
        command.Parameters.AddWithValue(quantity);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? ReadProduct(reader)
            : null;
    }

    private static Product ReadProduct(NpgsqlDataReader reader)
    {
        return new Product
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Price = reader.GetDecimal(2),
            Quantity = reader.GetInt32(3),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(4),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(5),
        };
    }
}
