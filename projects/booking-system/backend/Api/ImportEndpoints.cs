using AirportTicketBookingSystem.Services;

namespace AirportTicketBookingSystem.Api;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/manager/flights/import", ImportFlightsAsync);
        return endpoints;
    }

    private static async Task<IResult> ImportFlightsAsync(
        IFormFile file,
        ImportService service,
        CancellationToken ct)
    {
        if (file.Length == 0)
        {
            return Results.BadRequest(new { message = "Choose a non-empty CSV file." });
        }

        var result = await service.ImportFlightsAsync(file.OpenReadStream(), ct);
        return result.Errors.Count > 0 ? Results.BadRequest(result) : Results.Ok(result);
    }
}
