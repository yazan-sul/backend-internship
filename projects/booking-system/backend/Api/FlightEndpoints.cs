using System.Globalization;
using AirportTicketBookingSystem.Contracts;
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
            var query = request.Query;
            var errors = new Dictionary<string, string[]>();

            string? GetString(string key) =>
                query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                    ? value.ToString().Trim()
                    : null;

            decimal? GetDecimal(string key)
            {
                var raw = GetString(key);
                if (raw is null)
                {
                    return null;
                }

                if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value >= 0)
                {
                    return value;
                }

                errors[key] = [$"{key} must be a non-negative decimal number."];
                return null;
            }

            DateTime? departureDate = null;
            var rawDate = GetString("date");
            if (rawDate is not null)
            {
                if (DateTime.TryParseExact(rawDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    departureDate = parsedDate.Date;
                }
                else
                {
                    errors["date"] = ["date must use the YYYY-MM-DD format."];
                }
            }

            TravelClass? travelClass = null;
            var rawClass = GetString("class");
            if (rawClass is not null)
            {
                if (Enum.TryParse<TravelClass>(rawClass, true, out var parsedClass) && Enum.IsDefined(parsedClass))
                {
                    travelClass = parsedClass;
                }
                else
                {
                    errors["class"] = ["class must be Economy, Business, or First."];
                }
            }

            var departureCountry = GetString("departureCountry");
            var destinationCountry = GetString("destinationCountry");
            var departureAirport = GetString("departureAirport");
            var arrivalAirport = GetString("arrivalAirport");
            var minPrice = GetDecimal("minPrice");
            var maxPrice = GetDecimal("maxPrice");

            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            {
                errors["price"] = ["minPrice must be less than or equal to maxPrice."];
            }

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var flights = await repository.GetFlightsAsync(ct);

            var results = flights
                .Where(flight =>
                    (departureCountry is null || flight.DepartureCountry.Equals(departureCountry, StringComparison.OrdinalIgnoreCase)) &&
                    (destinationCountry is null || flight.DestinationCountry.Equals(destinationCountry, StringComparison.OrdinalIgnoreCase)) &&
                    (departureAirport is null || flight.DepartureAirport.Equals(departureAirport, StringComparison.OrdinalIgnoreCase)) &&
                    (arrivalAirport is null || flight.ArrivalAirport.Equals(arrivalAirport, StringComparison.OrdinalIgnoreCase)) &&
                    (!departureDate.HasValue || flight.DepartureAt.Date == departureDate) &&
                    (!minPrice.HasValue || PriceFor(flight, travelClass) >= minPrice) &&
                    (!maxPrice.HasValue || PriceFor(flight, travelClass) <= maxPrice) &&
                    (!travelClass.HasValue || flight.Remaining(travelClass.Value) > 0))
                .Where(flight => flight.DepartureAt > DateTime.UtcNow)
                .OrderBy(flight => flight.DepartureAt)
                .ToList();

            return Results.Ok(results.Select(flight => new FlightSearchResult(
                flight.Id,
                flight.Code,
                flight.DepartureCountry,
                flight.DestinationCountry,
                flight.DepartureAirport,
                flight.ArrivalAirport,
                flight.DepartureAt,
                new FlightPrices(flight.EconomyPrice, flight.BusinessPrice, flight.FirstPrice),
                new FlightAvailability(flight.EconomyRemaining, flight.BusinessRemaining, flight.FirstRemaining))));
        });

        return endpoints;
    }

    private static decimal PriceFor(Flight flight, TravelClass? travelClass) => travelClass switch
    {
        TravelClass.Business => flight.BusinessPrice,
        TravelClass.First => flight.FirstPrice,
        _ => flight.EconomyPrice
    };
}
