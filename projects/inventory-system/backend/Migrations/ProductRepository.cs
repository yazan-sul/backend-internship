using Npgsql;
using InventorySystem.DTOs;
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
    /// Persists a new product and returns the database-generated product record.
    /// </summary>
    public async Task<Product> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO products (name, price, quantity)
            VALUES ($1, $2, $3)
            RETURNING id, name, price, quantity, created_at, updated_at;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(request.Name!.Trim());
        command.Parameters.AddWithValue(request.Price);
        command.Parameters.AddWithValue(request.Quantity);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("The product was not created.");
        }

        return ReadProduct(reader);
    }

    /// Loads one sorted and paginated product page from the database.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the database operation.</param>
    /// <returns>A read-only list of products.</returns>
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPageAsync(
        string? search = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "name",
        string sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var orderBy = sortBy.ToLowerInvariant() switch
        {
            "price" => "price",
            "quantity" => "quantity",
            "inventoryvalue" => "price * quantity",
            _ => "name",
        };
        var direction = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        var offset = (page - 1) * pageSize;
        var sql = $"""
            SELECT id, name, price, quantity, created_at, updated_at, COUNT(*) OVER() AS total_count
            FROM products
            WHERE $1 = '' OR name ILIKE '%' || $1 || '%'
            ORDER BY {orderBy} {direction}, id ASC
            LIMIT $2 OFFSET $3;
            """;

        var products = new List<Product>();
        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(search?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue(pageSize);
        command.Parameters.AddWithValue(offset);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var totalCount = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(ReadProduct(reader));
            totalCount = reader.GetInt32(6);
        }

        return (products, totalCount);
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
    /// Deletes a product and reports whether a row was removed.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM products
            WHERE id = $1;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(id);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
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
