using System.ComponentModel.DataAnnotations;

namespace AirportTicketBookingSystem.Models;

public sealed class Passenger
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(120)]
    public string Name { get; set; } = "";

    [Required]
    public string? ContactDetails { get; set; }
}
