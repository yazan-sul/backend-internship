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
        // Product routes will be registered here in the next implementation step.
        return endpoints;
    }
}
