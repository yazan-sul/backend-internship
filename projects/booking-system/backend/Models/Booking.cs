namespace AirportTicketBookingSystem.Models;

public sealed class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PassengerId { get; set; }

    public Guid FlightId { get; set; }

    public TravelClass Class { get; set; }

    public decimal FinalPrice { get; set; }

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    public BookingStatus Status { get; set; } = BookingStatus.Active;
}
