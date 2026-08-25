using InventorySystem.Models;

namespace InventorySystem.Migrations;

/// <summary>
/// Encapsulates SQL access for products.
/// </summary>
/// <remarks>
/// Product queries will be implemented here when the product API is added.
/// </remarks>
public sealed class ProductRepository
{
    /// <summary>
    /// Loads all products from the database.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the database operation.</param>
    /// <returns>A read-only list of products.</returns>
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Product queries will be added with the product API.");
    }
}
