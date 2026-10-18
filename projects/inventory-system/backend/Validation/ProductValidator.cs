using InventorySystem.DTOs;

namespace InventorySystem.Validation;

/// <summary>
/// Provides shared validation rules for product input.
/// </summary>
public static class ProductValidator
{
    /// <summary>
    /// Validates the values shared by create and update requests.
    /// </summary>
    /// <returns>
    /// An error message when invalid; otherwise <see langword="null"/>.
    /// </returns>
    public static string? Validate(string? name, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Product name is required.";
        if (name.Trim().Length > 200) return "Product name must be 200 characters or fewer.";
        if (price < 0) return "Price must be non-negative.";
        if (quantity < 0) return "Quantity must be non-negative.";
        return null;
    }

    /// <summary>
    /// Validates a product creation request.
    /// </summary>
    public static string? Validate(CreateProductRequest request) =>
        Validate(request.Name, request.Price, request.Quantity);

    /// <summary>
    /// Validates a product update request.
    /// </summary>
    public static string? Validate(UpdateProductRequest request) =>
        Validate(request.Name, request.Price, request.Quantity);
}
