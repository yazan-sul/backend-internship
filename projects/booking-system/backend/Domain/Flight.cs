using System.ComponentModel.DataAnnotations;
namespace AirportTicketBookingSystem.Domain;
public sealed class Flight
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, StringLength(12)] public string Code { get; set; } = "";
    [Required, StringLength(80)] public string DepartureCountry { get; set; } = "";
    [Required, StringLength(80)] public string DestinationCountry { get; set; } = "";
    [Required, StringLength(12)] public string DepartureAirport { get; set; } = "";
    [Required, StringLength(12)] public string ArrivalAirport { get; set; } = "";
    public DateTime DepartureAt { get; set; }
    [Range(0.01, 100000)] public decimal EconomyPrice { get; set; }
    [Range(0.01, 100000)] public decimal BusinessPrice { get; set; }
    [Range(0.01, 100000)] public decimal FirstPrice { get; set; }
    [Range(0, 100000)] public int EconomyCapacity { get; set; }
    [Range(0, 100000)] public int BusinessCapacity { get; set; }
    [Range(0, 100000)] public int FirstCapacity { get; set; }
    public int EconomyRemaining { get; set; }
    public int BusinessRemaining { get; set; }
    public int FirstRemaining { get; set; }
    public decimal Price(TravelClass c) => c switch { TravelClass.Business => BusinessPrice, TravelClass.First => FirstPrice, _ => EconomyPrice };
    public int Remaining(TravelClass c) => c switch { TravelClass.Business => BusinessRemaining, TravelClass.First => FirstRemaining, _ => EconomyRemaining };
    public void ChangeSeats(TravelClass c, int amount) { if (c == TravelClass.Business) BusinessRemaining += amount; else if (c == TravelClass.First) FirstRemaining += amount; else EconomyRemaining += amount; }
}
