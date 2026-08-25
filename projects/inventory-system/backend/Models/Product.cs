namespace InventorySystem.Models;

/// <summary>
/// Represents a product persisted in the inventory database.
/// </summary>
public sealed class Product
{
    /// <summary>
    /// Gets the database-generated product identifier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets or sets the product's unique display name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the product's unit price in the configured currency.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the number of units currently in stock.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets the UTC timestamp when the product was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the product was last changed.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
