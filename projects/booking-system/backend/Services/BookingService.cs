using AirportTicketBookingSystem.Contracts;
using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Persistence;

namespace AirportTicketBookingSystem.Services;

public sealed class BookingService(PostgresRepository repository)
{
    public async Task<object> CreateAsync(BookingRequest request, CancellationToken ct)
    {
        var flight = (await repository.GetFlightsAsync(ct)).FirstOrDefault(x => x.Id == request.FlightId)
            ?? throw new InvalidOperationException("Flight not found.");
        if (flight.DepartureAt <= DateTime.UtcNow || flight.Remaining(request.Class) < 1)
        {
            throw new InvalidOperationException("This class is no longer available.");
        }

        var passenger = await repository.GetPassengerAsync(request.PassengerId, ct)
            ?? throw new InvalidOperationException("Passenger not found.");
        var booking = new Booking
        {
            PassengerId = passenger.Id,
            FlightId = flight.Id,
            Class = request.Class,
            FinalPrice = flight.Price(request.Class)
        };
        await repository.CreateBookingAsync(booking, ct);
        flight.ChangeSeats(request.Class, -1);
        return new { booking, flight, passenger };
    }

    public async Task<object?> CancelAsync(Guid id, Guid passengerId, CancellationToken ct)
        => await repository.CancelBookingAsync(id, passengerId, ct);

    public async Task<object?> ModifyAsync(Guid id, ModifyBookingRequest request, CancellationToken ct)
    {
        var booking = await repository.ModifyBookingAsync(id, request, ct);
        if (booking is null)
        {
            return null;
        }

        var flights = await repository.GetFlightsAsync(ct);
        var passengers = await repository.GetPassengersAsync(ct);
        return new
        {
            booking,
            flight = flights.FirstOrDefault(flight => flight.Id == booking.FlightId),
            passenger = passengers.FirstOrDefault(passenger => passenger.Id == booking.PassengerId)
        };
    }
}
