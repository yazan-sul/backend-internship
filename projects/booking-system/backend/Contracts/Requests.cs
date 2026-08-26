using AirportTicketBookingSystem.Models;
namespace AirportTicketBookingSystem.Contracts;
public sealed record BookingRequest(Guid FlightId, TravelClass Class, string Name, string Email, string ContactDetails);
public sealed record PassengerRequest(string Name, string Email, string ContactDetails);
public sealed record BookingQuery(string? Flight, decimal? MinPrice, decimal? MaxPrice, string? DepartureCountry, string? DestinationCountry, string? DepartureAirport, string? ArrivalAirport, DateTime? Date, TravelClass? Class, string? Passenger);
