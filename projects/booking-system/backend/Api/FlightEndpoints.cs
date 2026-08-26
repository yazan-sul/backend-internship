using System.Globalization;
using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Persistence;

namespace AirportTicketBookingSystem.Api;

public static class FlightEndpoints
{
    public static IEndpointRouteBuilder MapFlightEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/flights", async (
            HttpRequest request,
            PostgresRepository repository,
            CancellationToken ct) =>
        {
            var flights = await repository.GetFlightsAsync(ct);
            var query = request.Query;

            string? GetString(string key) =>
                query.TryGetValue(key, out var value) ? value.ToString() : null;

            decimal? GetDecimal(string key) =>
                decimal.TryParse(GetString(key), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : null;

            var departureDate = DateTime.TryParse(GetString("date"), out var parsedDate)
                ? parsedDate.Date
                : (DateTime?)null;

            var travelClass = Enum.TryParse<TravelClass>(GetString("class"), true, out var parsedClass)
                ? parsedClass
                : (TravelClass?)null;

            var departureCountry = GetString("departureCountry");
            var destinationCountry = GetString("destinationCountry");
            var departureAirport = GetString("departureAirport");
            var arrivalAirport = GetString("arrivalAirport");
            var minPrice = GetDecimal("minPrice");
            var maxPrice = GetDecimal("maxPrice");

            var results = flights
                .Where(flight =>
                    (departureCountry is null || flight.DepartureCountry.Equals(departureCountry, StringComparison.OrdinalIgnoreCase)) &&
                    (destinationCountry is null || flight.DestinationCountry.Equals(destinationCountry, StringComparison.OrdinalIgnoreCase)) &&
                    (departureAirport is null || flight.DepartureAirport.Equals(departureAirport, StringComparison.OrdinalIgnoreCase)) &&
                    (arrivalAirport is null || flight.ArrivalAirport.Equals(arrivalAirport, StringComparison.OrdinalIgnoreCase)) &&
                    (!departureDate.HasValue || flight.DepartureAt.Date == departureDate) &&
                    (!minPrice.HasValue || flight.EconomyPrice >= minPrice) &&
                    (!maxPrice.HasValue || flight.EconomyPrice <= maxPrice) &&
                    (!travelClass.HasValue || flight.Remaining(travelClass.Value) > 0))
                .Where(flight => flight.DepartureAt > DateTime.UtcNow)
                .OrderBy(flight => flight.DepartureAt)
                .ToList();

            return Results.Ok(results.Select(flight => new
            {
                flight.Id,
                flight.Code,
                flight.DepartureCountry,
                flight.DestinationCountry,
                flight.DepartureAirport,
                flight.ArrivalAirport,
                flight.DepartureAt,
                prices = new
                {
                    economy = flight.EconomyPrice,
                    business = flight.BusinessPrice,
                    first = flight.FirstPrice
                },
                availability = new
                {
                    economy = flight.EconomyRemaining,
                    business = flight.BusinessRemaining,
                    first = flight.FirstRemaining
                }
            }));
        });

        return endpoints;
    }
}
