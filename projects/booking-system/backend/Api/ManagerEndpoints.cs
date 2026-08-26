using System.Globalization;
using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Persistence;
using AirportTicketBookingSystem.Validation;

namespace AirportTicketBookingSystem.Api;

public static class ManagerEndpoints
{
    public static IEndpointRouteBuilder MapManagerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/manager/bookings", async (
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
                if (raw is null) return null;
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

            var minPrice = GetDecimal("minPrice");
            var maxPrice = GetDecimal("maxPrice");
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            {
                errors["price"] = ["minPrice must be less than or equal to maxPrice."];
            }

            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var flightFilter = GetString("flight");
            var departureCountry = GetString("departureCountry");
            var destinationCountry = GetString("destinationCountry");
            var departureAirport = GetString("departureAirport");
            var arrivalAirport = GetString("arrivalAirport");
            var passengerFilter = GetString("passenger");
            var flights = await repository.GetFlightsAsync(ct);
            var passengers = await repository.GetPassengersAsync(ct);
            var bookings = await repository.GetBookingsAsync(ct);

            var results = bookings
                .Select(booking => new
                {
                    booking,
                    flight = flights.FirstOrDefault(flight => flight.Id == booking.FlightId),
                    passenger = passengers.FirstOrDefault(passenger => passenger.Id == booking.PassengerId)
                })
                .Where(item => item.flight is not null && item.passenger is not null)
                .Where(item =>
                    (flightFilter is null || item.flight!.Code.Contains(flightFilter, StringComparison.OrdinalIgnoreCase)) &&
                    (!minPrice.HasValue || item.booking.FinalPrice >= minPrice) &&
                    (!maxPrice.HasValue || item.booking.FinalPrice <= maxPrice) &&
                    (departureCountry is null || item.flight!.DepartureCountry.Equals(departureCountry, StringComparison.OrdinalIgnoreCase)) &&
                    (destinationCountry is null || item.flight!.DestinationCountry.Equals(destinationCountry, StringComparison.OrdinalIgnoreCase)) &&
                    (departureAirport is null || item.flight!.DepartureAirport.Equals(departureAirport, StringComparison.OrdinalIgnoreCase)) &&
                    (arrivalAirport is null || item.flight!.ArrivalAirport.Equals(arrivalAirport, StringComparison.OrdinalIgnoreCase)) &&
                    (!departureDate.HasValue || item.flight!.DepartureAt.Date == departureDate) &&
                    (!travelClass.HasValue || item.booking.Class == travelClass) &&
                    (passengerFilter is null ||
                        item.passenger!.Name.Contains(passengerFilter, StringComparison.OrdinalIgnoreCase) ||
                        (item.passenger.ContactDetails?.Contains(passengerFilter, StringComparison.OrdinalIgnoreCase) ?? false)))
                .OrderByDescending(item => item.booking.BookedAt)
                .ToList();

            return Results.Ok(results);
        });

        endpoints.MapGet(
            "/api/manager/flights/validation-details",
            (ValidationMetadataProvider provider) => Results.Ok(provider.Details<Flight>()));

        return endpoints;
    }
}
