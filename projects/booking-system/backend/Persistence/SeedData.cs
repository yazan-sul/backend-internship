using AirportTicketBookingSystem.Models;

namespace AirportTicketBookingSystem.Persistence;

public static class SeedData
{
    public static List<Flight> Flights() =>
    [
        new Flight
        {
            Code = "SB101",
            DepartureCountry = "Jordan",
            DestinationCountry = "France",
            DepartureAirport = "AMM",
            ArrivalAirport = "CDG",
            DepartureAt = DateTime.UtcNow.AddDays(12),
            EconomyPrice = 220,
            BusinessPrice = 580,
            FirstPrice = 1100,
            EconomyCapacity = 80,
            BusinessCapacity = 12,
            FirstCapacity = 4,
            EconomyRemaining = 80,
            BusinessRemaining = 12,
            FirstRemaining = 4
        },
        new Flight
        {
            Code = "SB202",
            DepartureCountry = "Jordan",
            DestinationCountry = "Italy",
            DepartureAirport = "AMM",
            ArrivalAirport = "FCO",
            DepartureAt = DateTime.UtcNow.AddDays(20),
            EconomyPrice = 180,
            BusinessPrice = 450,
            FirstPrice = 900,
            EconomyCapacity = 90,
            BusinessCapacity = 10,
            FirstCapacity = 2,
            EconomyRemaining = 90,
            BusinessRemaining = 10,
            FirstRemaining = 2
        }
    ];
}
