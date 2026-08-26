using System.ComponentModel.DataAnnotations;
using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Validation;
using Xunit;

namespace AirportTicketBookingSystem.Tests;

public sealed class ValidationMetadataProviderTests
{
    private readonly ValidationMetadataProvider provider = new();

    [Fact]
    public void Flight_metadata_includes_model_constraints_and_business_rules()
    {
        var code = Assert.Single(provider.Details<Flight>(), field => field.Field == nameof(Flight.Code));
        var departure = Assert.Single(provider.Details<Flight>(), field => field.Field == nameof(Flight.DepartureAt));
        var economyPrice = Assert.Single(provider.Details<Flight>(), field => field.Field == nameof(Flight.EconomyPrice));

        Assert.Equal("Flight code", code.DisplayName);
        Assert.True(code.Required);
        Assert.Equal(12, code.MaxLength);
        Assert.Contains("Must be unique", Assert.Single(code.CustomRules));
        Assert.Contains("later than", Assert.Single(departure.CustomRules));
        Assert.Equal(0.01m, Convert.ToDecimal(economyPrice.Min));
        Assert.Equal(100000m, Convert.ToDecimal(economyPrice.Max));
    }

    [Fact]
    public void Changing_an_annotation_changes_reported_metadata()
    {
        var before = Assert.Single(provider.Details<AnnotatedFlight>(), field => field.Field == nameof(AnnotatedFlight.Code));

        Assert.Equal(8, before.MaxLength);
        Assert.Equal("Annotated flight code", before.DisplayName);
        Assert.Contains("Use uppercase letters.", before.CustomRules);
    }

    [Fact]
    public void Flight_business_rules_are_enforced_by_model_validation()
    {
        var flight = new Flight
        {
            Code = "SKY-1",
            DepartureCountry = "Palestine",
            DestinationCountry = "Palestine",
            DepartureAirport = "JFK",
            ArrivalAirport = "JFK",
            DepartureAt = DateTime.UtcNow.AddMinutes(-1),
            EconomyPrice = 100,
            BusinessPrice = 200,
            FirstPrice = 300
        };

        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(flight, new ValidationContext(flight), errors, true);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(Flight.DepartureAt)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(Flight.DestinationCountry)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(Flight.ArrivalAirport)));
    }

    [Fact]
    public void Travel_class_enum_options_are_reported()
    {
        var details = provider.Details<EnumCarrier>();
        var classField = Assert.Single(details, field => field.Field == nameof(EnumCarrier.Class));

        Assert.Equal("TravelClass", classField.Type);
        Assert.Equal(["Economy", "Business", "First"], classField.Options);
    }

    [Fact]
    public void Passenger_constraints_reject_missing_name_and_contact_details()
    {
        var passenger = new Passenger();
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(passenger, new ValidationContext(passenger), errors, true);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(Passenger.Name)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(Passenger.ContactDetails)));
    }

    private sealed class AnnotatedFlight
    {
        [Display(Name = "Annotated flight code"), StringLength(8)]
        [ValidationRule("Use uppercase letters.")]
        public string Code { get; set; } = "";
    }

    private sealed class EnumCarrier
    {
        public TravelClass Class { get; set; }
    }
}
