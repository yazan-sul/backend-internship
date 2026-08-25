using System.ComponentModel.DataAnnotations;
namespace AirportTicketBookingSystem.Domain;
public sealed class Passenger { public Guid Id { get; set; } = Guid.NewGuid(); [Required, StringLength(120)] public string Name { get; set; } = ""; [Required, EmailAddress] public string Email { get; set; } = ""; [Required] public string ContactDetails { get; set; } = ""; }
