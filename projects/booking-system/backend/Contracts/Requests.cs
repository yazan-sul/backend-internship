using AirportTicketBookingSystem.Models;
namespace AirportTicketBookingSystem.Contracts;
public sealed record BookingRequest(Guid PassengerId, Guid FlightId, TravelClass Class);

public sealed record PassengerRequest(string Name, string? ContactDetails);

public sealed record BookingQuery(
    string? Flight,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? DepartureCountry,
    string? DestinationCountry,
    string? DepartureAirport,
    string? ArrivalAirport,
    DateTime? Date,
    TravelClass? Class,
    string? Passenger);

public sealed record FlightSearchCriteria(
    string? DepartureCountry,
    string? DestinationCountry,
    string? DepartureAirport,
    string? ArrivalAirport,
    DateTime? DepartureDate,
    decimal? MinPrice,
    decimal? MaxPrice,
    TravelClass? Class);

public sealed record FlightSearchResult(
    Guid Id,
    string Code,
    string DepartureCountry,
    string DestinationCountry,
    string DepartureAirport,
    string ArrivalAirport,
    DateTime DepartureAt,
    FlightPrices Prices,
    FlightAvailability Availability);

public sealed record FlightPrices(decimal Economy, decimal Business, decimal First);

public sealed record FlightAvailability(int Economy, int Business, int First);
