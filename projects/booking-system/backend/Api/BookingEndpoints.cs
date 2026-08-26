using AirportTicketBookingSystem.Contracts;
using AirportTicketBookingSystem.Persistence;
using AirportTicketBookingSystem.Services;

namespace AirportTicketBookingSystem.Api;

public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/bookings", async (
            BookingRequest request,
            BookingService service,
            CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await service.CreateAsync(request, ct));
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        });

        endpoints.MapGet("/api/bookings/me", async (
            Guid passengerId,
            PostgresRepository repository,
            CancellationToken ct) =>
        {
            if (await repository.GetPassengerAsync(passengerId, ct) is null)
            {
                return Results.Ok(Array.Empty<object>());
            }

            var flights = await repository.GetFlightsAsync(ct);
            var bookings = await repository.GetBookingsAsync(ct);

            return Results.Ok(bookings
                .Where(booking => booking.PassengerId == passengerId)
                .Select(booking => new
                {
                    booking,
                    flight = flights.FirstOrDefault(flight => flight.Id == booking.FlightId)
                }));
        });

        endpoints.MapPut("/api/bookings/{id:guid}", async (
            Guid id,
            ModifyBookingRequest request,
            BookingService service,
            CancellationToken ct) =>
        {
            try
            {
                var result = await service.ModifyAsync(id, request, ct);
                return result is null
                    ? Results.NotFound(new { message = "Booking not found." })
                    : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        });

        endpoints.MapPost("/api/bookings/{id:guid}/cancel", async (
            Guid id,
            Guid passengerId,
            BookingService service,
            CancellationToken ct) =>
        {
            try
            {
                var result = await service.CancelAsync(id, passengerId, ct);

                return result is null
                    ? Results.NotFound(new { message = "Booking not found." })
                    : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        });

        return endpoints;
    }
}
