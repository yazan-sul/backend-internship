using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Persistence;
using AirportTicketBookingSystem.Validation;

namespace AirportTicketBookingSystem.Api;

public static class ManagerEndpoints
{
    public static IEndpointRouteBuilder MapManagerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/manager/bookings", async (
            PostgresRepository repository,
            CancellationToken ct) =>
        {
            var flights = await repository.GetFlightsAsync(ct);
            var passengers = await repository.GetPassengersAsync(ct);
            var bookings = await repository.GetBookingsAsync(ct);

            return Results.Ok(bookings.Select(booking => new
            {
                booking,
                flight = flights.FirstOrDefault(flight => flight.Id == booking.FlightId),
                passenger = passengers.FirstOrDefault(passenger => passenger.Id == booking.PassengerId)
            }));
        });

        endpoints.MapGet(
            "/api/manager/flights/validation-details",
            (ValidationMetadataProvider provider) => Results.Ok(provider.Details<Flight>()));

        return endpoints;
    }
}
