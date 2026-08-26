using System.ComponentModel.DataAnnotations;
namespace AirportTicketBookingSystem.Models;

public sealed class Flight : IValidatableObject
{
    [Display(Name = "Flight ID")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Display(Name = "Flight code"), Required, StringLength(12)]
    [ValidationRule("Must be unique across imported and existing flights.")]
    public string Code { get; set; } = "";

    [Display(Name = "Departure country"), Required, StringLength(80)]
    public string DepartureCountry { get; set; } = "";

    [Display(Name = "Destination country"), Required, StringLength(80)]
    [ValidationRule("Must differ from the departure country.")]
    public string DestinationCountry { get; set; } = "";

    [Display(Name = "Departure airport"), Required, StringLength(12)]
    public string DepartureAirport { get; set; } = "";

    [Display(Name = "Arrival airport"), Required, StringLength(12)]
    [ValidationRule("Must differ from the departure airport.")]
    public string ArrivalAirport { get; set; } = "";

    [Display(Name = "Departure date and time")]
    [ValidationRule("Must be later than the current UTC date and time.")]
    public DateTime DepartureAt { get; set; }

    [Display(Name = "Economy price"), Range(0.01, 100000)]
    public decimal EconomyPrice { get; set; }

    [Display(Name = "Business price"), Range(0.01, 100000)]
    public decimal BusinessPrice { get; set; }

    [Display(Name = "First price"), Range(0.01, 100000)]
    public decimal FirstPrice { get; set; }

    [Display(Name = "Economy capacity"), Range(0, 100000)]
    public int EconomyCapacity { get; set; }

    [Display(Name = "Business capacity"), Range(0, 100000)]
    public int BusinessCapacity { get; set; }

    [Display(Name = "First capacity"), Range(0, 100000)]
    public int FirstCapacity { get; set; }

    [Display(Name = "Economy seats remaining")]
    public int EconomyRemaining { get; set; }
    [Display(Name = "Business seats remaining")]
    public int BusinessRemaining { get; set; }
    [Display(Name = "First seats remaining")]
    public int FirstRemaining { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DepartureAt != default && DepartureAt <= DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "Departure must be later than the current UTC date and time.",
                [nameof(DepartureAt)]);
        }

        if (!string.IsNullOrWhiteSpace(DepartureCountry) &&
            DepartureCountry.Equals(DestinationCountry, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Departure and destination countries must differ.",
                [nameof(DestinationCountry)]);
        }

        if (!string.IsNullOrWhiteSpace(DepartureAirport) &&
            DepartureAirport.Equals(ArrivalAirport, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Departure and arrival airports must differ.",
                [nameof(ArrivalAirport)]);
        }
    }

    public decimal Price(TravelClass travelClass) => travelClass switch
    {
        TravelClass.Business => BusinessPrice,
        TravelClass.First => FirstPrice,
        _ => EconomyPrice
    };

    public int Remaining(TravelClass travelClass) => travelClass switch
    {
        TravelClass.Business => BusinessRemaining,
        TravelClass.First => FirstRemaining,
        _ => EconomyRemaining
    };

    public void ChangeSeats(TravelClass travelClass, int amount)
    {
        if (travelClass == TravelClass.Business)
        {
            BusinessRemaining += amount;
        }
        else if (travelClass == TravelClass.First)
        {
            FirstRemaining += amount;
        }
        else
        {
            EconomyRemaining += amount;
        }
    }
}
