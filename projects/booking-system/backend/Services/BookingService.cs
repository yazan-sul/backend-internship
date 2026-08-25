using AirportTicketBookingSystem.Contracts;
using AirportTicketBookingSystem.Domain;
using AirportTicketBookingSystem.Persistence;
namespace AirportTicketBookingSystem.Services;
public sealed class BookingService(JsonFileRepository<Flight> flights, JsonFileRepository<Passenger> passengers, JsonFileRepository<Booking> bookings)
{
    public async Task<object> CreateAsync(BookingRequest request, CancellationToken ct)
    {
        var fs = await flights.ReadAsync(ct); var ps = await passengers.ReadAsync(ct); var bs = await bookings.ReadAsync(ct);
        var f = fs.FirstOrDefault(x => x.Id == request.FlightId) ?? throw new InvalidOperationException("Flight not found.");
        if (f.DepartureAt <= DateTime.UtcNow || f.Remaining(request.Class) < 1) throw new InvalidOperationException("This class is no longer available.");
        var p = ps.FirstOrDefault(x => x.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (p is null) { p = new Passenger { Name = request.Name.Trim(), Email = request.Email.Trim().ToLowerInvariant(), ContactDetails = request.ContactDetails.Trim() }; ps.Add(p); }
        f.ChangeSeats(request.Class, -1); var b = new Booking { PassengerId = p.Id, FlightId = f.Id, Class = request.Class, FinalPrice = f.Price(request.Class) }; bs.Add(b);
        await flights.WriteAsync(fs, ct); await passengers.WriteAsync(ps, ct); await bookings.WriteAsync(bs, ct); return new { booking = b, flight = f, passenger = p };
    }
    public async Task<object?> CancelAsync(Guid id, string email, CancellationToken ct) { var fs = await flights.ReadAsync(ct); var bs = await bookings.ReadAsync(ct); var ps = await passengers.ReadAsync(ct); var b = bs.FirstOrDefault(x => x.Id == id); var p = ps.FirstOrDefault(x => x.Id == b?.PassengerId); if (b is null || p is null || !p.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) return null; if (b.Status == BookingStatus.Cancelled) return b; var f = fs.FirstOrDefault(x => x.Id == b.FlightId); if (f is null || f.DepartureAt <= DateTime.UtcNow) throw new InvalidOperationException("The flight has already departed."); b.Status = BookingStatus.Cancelled; f.ChangeSeats(b.Class, 1); await flights.WriteAsync(fs, ct); await bookings.WriteAsync(bs, ct); return b; }
}
