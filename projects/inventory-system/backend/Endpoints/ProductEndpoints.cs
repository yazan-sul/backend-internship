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

        products.MapGet("", async (
            string? search,
            int? page,
            int? pageSize,
            string? sortBy,
            string? sortDirection,
            ProductRepository repository,
            CancellationToken cancellationToken) =>
        {
            var currentPage = Math.Max(page ?? 1, 1);
            var currentPageSize = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await repository.GetPageAsync(
                search,
                currentPage,
                currentPageSize,
                sortBy ?? "name",
                sortDirection ?? "asc",
                cancellationToken);
            return Results.Ok(new ProductPageResponse(
                result.Items.Select(ToResponse).ToList(),
                result.TotalCount,
                currentPage,
                currentPageSize));
        });

        products.MapGet("/{id:int}", async (int id, ProductRepository repository, CancellationToken cancellationToken) =>
        {
            var product = await repository.GetByIdAsync(id, cancellationToken);
            return product is null
                ? Results.NotFound(new { message = $"Product with ID {id} was not found." })
                : Results.Ok(ToResponse(product));
        });

        products.MapPut("/{id:int}", async (
            int id,
            UpdateProductRequest request,
            ProductRepository repository,
            CancellationToken cancellationToken) =>
        {
            var validationError = ProductValidator.Validate(request);
            if (validationError is not null)
            {
                return Results.BadRequest(new { message = validationError });
            }

            try
            {
                var product = await repository.UpdateAsync(
                    id,
                    request.Name!.Trim(),
                    request.Price,
                    request.Quantity,
                    cancellationToken);

                return product is null
                    ? Results.NotFound(new { message = $"Product with ID {id} was not found." })
                    : Results.Ok(ToResponse(product));
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Results.Conflict(new { message = "Another product already uses this name." });
            }
        });

        products.MapDelete("/{id:int}", async (
            int id,
            ProductRepository repository,
            CancellationToken cancellationToken) =>
        {
            var deleted = await repository.DeleteAsync(id, cancellationToken);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new { message = $"Product with ID {id} was not found." });
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
