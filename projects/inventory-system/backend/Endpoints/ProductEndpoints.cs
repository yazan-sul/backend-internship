using InventorySystem.DTOs;
using InventorySystem.Migrations;
using InventorySystem.Models;

namespace InventorySystem.Endpoints;

/// <summary>
/// Registers HTTP endpoints for product operations.
/// </summary>
public static class ProductEndpoints
{
    /// <summary>
    /// Maps product routes onto the application's endpoint pipeline.
    /// </summary>
    /// <param name="endpoints">The application route builder.</param>
    /// <returns>The same route builder for fluent registration.</returns>
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var products = endpoints.MapGroup("/api/products");

        products.MapGet("", async (ProductRepository repository, CancellationToken cancellationToken) =>
        {
            var productList = await repository.GetAllAsync(cancellationToken);
            var response = productList.Select(ToResponse).ToList();
            return Results.Ok(response);
        });

        products.MapGet("/{id:int}", async (int id, ProductRepository repository, CancellationToken cancellationToken) =>
        {
            var product = await repository.GetByIdAsync(id, cancellationToken);
            return product is null
                ? Results.NotFound(new { message = $"Product with ID {id} was not found." })
                : Results.Ok(ToResponse(product));
        });

        return endpoints;
    }

    private static ProductResponse ToResponse(Product product) => new(
        product.Id,
        product.Name,
        product.Price,
        product.Quantity,
        product.CreatedAt,
        product.UpdatedAt);
}
