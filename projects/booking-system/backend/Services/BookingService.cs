using AirportTicketBookingSystem.Contracts;
using AirportTicketBookingSystem.Domain;
using AirportTicketBookingSystem.Persistence;

namespace AirportTicketBookingSystem.Services;

public sealed class BookingService(PostgresRepository repository)
{
    public async Task<object> CreateAsync(BookingRequest request, CancellationToken ct)
    {
        var flight = (await repository.GetFlightsAsync(ct)).FirstOrDefault(x => x.Id == request.FlightId) ?? throw new InvalidOperationException("Flight not found.");
        if (flight.DepartureAt <= DateTime.UtcNow || flight.Remaining(request.Class) < 1) throw new InvalidOperationException("This class is no longer available.");
        var passenger = await repository.FindPassengerAsync(request.Email.Trim(), ct) ?? new Passenger { Name = request.Name.Trim(), Email = request.Email.Trim().ToLowerInvariant(), ContactDetails = request.ContactDetails.Trim() };
        var booking = new Booking { PassengerId = passenger.Id, FlightId = flight.Id, Class = request.Class, FinalPrice = flight.Price(request.Class) };
        await repository.CreateBookingAsync(booking, passenger, ct);
        flight.ChangeSeats(request.Class, -1);
        return new { booking, flight, passenger };
    }

    public async Task<object?> CancelAsync(Guid id, string email, CancellationToken ct)
        => await repository.CancelBookingAsync(id, email, ct);
}
