using InventorySystem.DTOs;
using InventorySystem.Migrations;
using InventorySystem.Models;
using InventorySystem.Validation;
using Npgsql;

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

        products.MapPost("", async (CreateProductRequest request, ProductRepository repository, CancellationToken cancellationToken) =>
        {
            var validationError = ProductValidator.Validate(request);
            if (validationError is not null)
            {
                return Results.BadRequest(new { message = validationError });
            }

            try
            {
                var product = await repository.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/products/{product.Id}", ToResponse(product));
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Results.Conflict(new { message = "Another product already uses this name." });
            }
        });

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
