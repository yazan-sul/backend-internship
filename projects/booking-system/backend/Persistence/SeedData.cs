using AirportTicketBookingSystem.Models;

namespace AirportTicketBookingSystem.Persistence;

public static class SeedData
{
    public static List<Flight> Flights() =>
    [
        CreateFlight("SB101", "Jordan", "France", "AMM", "CDG", 12, 220, 580, 1100, 80, 12, 4),
        CreateFlight("SB102", "Jordan", "France", "AMM", "ORY", 19, 195, 520, 980, 72, 10, 3),
        CreateFlight("SB103", "Jordan", "France", "AMM", "NCE", 27, 240, 630, 1180, 68, 9, 3),
        CreateFlight("SB104", "Jordan", "France", "AMM", "LYS", 34, 205, 545, 1020, 76, 10, 3),
        CreateFlight("SB105", "Jordan", "France", "AMM", "MRS", 48, 230, 600, 1120, 64, 8, 2),

        CreateFlight("SB201", "Jordan", "Italy", "AMM", "FCO", 20, 180, 450, 900, 90, 10, 2),
        CreateFlight("SB202", "Jordan", "Italy", "AMM", "MXP", 15, 175, 440, 880, 88, 10, 2),
        CreateFlight("SB203", "Jordan", "Italy", "AMM", "VCE", 31, 210, 510, 980, 70, 8, 2),
        CreateFlight("SB204", "Jordan", "Italy", "AMM", "NAP", 42, 190, 470, 920, 74, 9, 2),
        CreateFlight("SB205", "Jordan", "Italy", "AMM", "BLQ", 56, 200, 500, 950, 66, 8, 2),

        CreateFlight("SB301", "Jordan", "Spain", "AMM", "MAD", 14, 260, 650, 1250, 82, 12, 4),
        CreateFlight("SB302", "Jordan", "Spain", "AMM", "BCN", 23, 245, 620, 1200, 86, 12, 4),
        CreateFlight("SB303", "Jordan", "Spain", "AMM", "AGP", 38, 275, 690, 1320, 70, 10, 3),
        CreateFlight("SB304", "Jordan", "Spain", "AMM", "SVQ", 51, 255, 640, 1220, 62, 8, 2),
        CreateFlight("SB305", "Jordan", "Spain", "AMM", "VLC", 64, 235, 590, 1140, 68, 9, 3),

        CreateFlight("SB401", "Jordan", "United Kingdom", "AMM", "LHR", 16, 290, 760, 1450, 94, 14, 4),
        CreateFlight("SB402", "Jordan", "United Kingdom", "AMM", "LGW", 25, 275, 720, 1380, 88, 12, 4),
        CreateFlight("SB403", "Jordan", "United Kingdom", "AMM", "MAN", 36, 265, 690, 1320, 80, 11, 3),
        CreateFlight("SB404", "Jordan", "United Kingdom", "AMM", "EDI", 47, 300, 780, 1500, 72, 10, 3),
        CreateFlight("SB405", "Jordan", "United Kingdom", "AMM", "BHX", 69, 255, 660, 1260, 76, 10, 3),

        CreateFlight("SB501", "Jordan", "Germany", "AMM", "FRA", 18, 250, 640, 1240, 90, 13, 4),
        CreateFlight("SB502", "Jordan", "Germany", "AMM", "MUC", 29, 265, 680, 1300, 84, 12, 4),
        CreateFlight("SB503", "Jordan", "Germany", "AMM", "BER", 40, 240, 610, 1180, 78, 10, 3),
        CreateFlight("SB504", "Jordan", "Germany", "AMM", "DUS", 53, 235, 600, 1160, 70, 9, 3),
        CreateFlight("SB505", "Jordan", "Germany", "AMM", "HAM", 75, 245, 625, 1200, 68, 9, 2),

        CreateFlight("SB601", "Jordan", "Greece", "AMM", "ATH", 10, 160, 390, 760, 92, 10, 2),
        CreateFlight("SB602", "Jordan", "Greece", "AMM", "SKG", 22, 150, 370, 720, 78, 8, 2),
        CreateFlight("SB603", "Jordan", "Greece", "AMM", "HER", 33, 175, 430, 820, 74, 8, 2),
        CreateFlight("SB604", "Jordan", "Greece", "AMM", "RHO", 45, 185, 450, 860, 70, 8, 2),
        CreateFlight("SB605", "Jordan", "Greece", "AMM", "JTR", 61, 210, 520, 990, 60, 7, 2),

        CreateFlight("SB701", "Jordan", "Turkey", "AMM", "IST", 13, 145, 360, 700, 96, 11, 2),
        CreateFlight("SB702", "Jordan", "Turkey", "AMM", "SAW", 26, 135, 340, 660, 90, 10, 2),
        CreateFlight("SB703", "Jordan", "Turkey", "AMM", "AYT", 37, 155, 390, 760, 82, 9, 2),
        CreateFlight("SB704", "Jordan", "Turkey", "AMM", "ADB", 58, 150, 375, 730, 76, 8, 2),
        CreateFlight("SB705", "Jordan", "Turkey", "AMM", "ESB", 72, 140, 350, 680, 72, 8, 2),

        CreateFlight("SB801", "Jordan", "United Arab Emirates", "AMM", "DXB", 17, 230, 590, 1150, 100, 14, 4),
        CreateFlight("SB802", "Jordan", "United Arab Emirates", "AMM", "AUH", 30, 220, 570, 1100, 92, 12, 4),
        CreateFlight("SB803", "Jordan", "United Arab Emirates", "AMM", "SHJ", 44, 205, 530, 1020, 86, 11, 3),
        CreateFlight("SB804", "Jordan", "United Arab Emirates", "AMM", "DWC", 67, 215, 550, 1060, 80, 10, 3),
        CreateFlight("SB805", "Jordan", "United Arab Emirates", "AMM", "RKT", 78, 195, 500, 960, 70, 9, 2)
    ];

    private static Flight CreateFlight(
        string code,
        string departureCountry,
        string destinationCountry,
        string departureAirport,
        string arrivalAirport,
        int daysFromNow,
        decimal economyPrice,
        decimal businessPrice,
        decimal firstPrice,
        int economyCapacity,
        int businessCapacity,
        int firstCapacity) => new()
    {
        Code = code,
        DepartureCountry = departureCountry,
        DestinationCountry = destinationCountry,
        DepartureAirport = departureAirport,
        ArrivalAirport = arrivalAirport,
        DepartureAt = DateTime.UtcNow.AddDays(daysFromNow),
        EconomyPrice = economyPrice,
        BusinessPrice = businessPrice,
        FirstPrice = firstPrice,
        EconomyCapacity = economyCapacity,
        BusinessCapacity = businessCapacity,
        FirstCapacity = firstCapacity,
        EconomyRemaining = economyCapacity,
        BusinessRemaining = businessCapacity,
        FirstRemaining = firstCapacity
    };
}
