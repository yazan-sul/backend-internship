namespace InventorySystem.DTOs;

/// <summary>
/// Represents the product shape returned by the API.
/// </summary>
public sealed record ProductResponse(
    int Id,
    string Name,
    decimal Price,
    int Quantity,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Contains the fields required to create a product.
/// </summary>
public sealed record CreateProductRequest(string? Name, decimal Price, int Quantity);

/// <summary>
/// Contains the fields that can be changed on an existing product.
/// </summary>
public sealed record UpdateProductRequest(string? Name, decimal Price, int Quantity);
