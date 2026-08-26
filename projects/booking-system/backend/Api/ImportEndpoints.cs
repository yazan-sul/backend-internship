using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Persistence;

namespace AirportTicketBookingSystem.Api;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/manager/flights/import", ImportFlightsAsync);
        return endpoints;
    }

    private static async Task<IResult> ImportFlightsAsync(
        IFormFile file,
        PostgresRepository repository,
        CancellationToken ct)
    {
        if (file.Length == 0)
        {
            return Results.BadRequest(new { message = "Choose a non-empty CSV file." });
        }

        var errors = new List<object>();
        var imported = new List<Flight>();

        using var reader = new StreamReader(file.OpenReadStream());
        await reader.ReadLineAsync(ct);

        var line = 0;
        while (await reader.ReadLineAsync(ct) is { } row)
        {
            line++;

            if (string.IsNullOrWhiteSpace(row))
            {
                continue;
            }

            var columns = row.Split(',');

            if (columns.Length != 11)
            {
                errors.Add(new
                {
                    row = line + 1,
                    field = "row",
                    value = row,
                    message = "Expected 11 columns."
                });
                continue;
            }

            DateTime departure = default;
            decimal economyPrice = default;
            decimal businessPrice = default;
            decimal firstPrice = default;
            int economyCapacity = default;
            int businessCapacity = default;

            var validValues =
                DateTime.TryParse(columns[5], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out departure) &&
                decimal.TryParse(columns[6], NumberStyles.Number, CultureInfo.InvariantCulture, out economyPrice) &&
                decimal.TryParse(columns[7], NumberStyles.Number, CultureInfo.InvariantCulture, out businessPrice) &&
                decimal.TryParse(columns[8], NumberStyles.Number, CultureInfo.InvariantCulture, out firstPrice) &&
                int.TryParse(columns[9], out economyCapacity) &&
                int.TryParse(columns[10], out businessCapacity);

            if (!validValues)
            {
                errors.Add(new
                {
                    row = line + 1,
                    field = "values",
                    value = row,
                    message = "Invalid date, price, or capacity."
                });
                continue;
            }

            var flight = new Flight
            {
                Code = columns[0].Trim(),
                DepartureCountry = columns[1].Trim(),
                DestinationCountry = columns[2].Trim(),
                DepartureAirport = columns[3].Trim(),
                ArrivalAirport = columns[4].Trim(),
                DepartureAt = departure.ToUniversalTime(),
                EconomyPrice = economyPrice,
                BusinessPrice = businessPrice,
                FirstPrice = firstPrice,
                EconomyCapacity = economyCapacity,
                BusinessCapacity = businessCapacity,
                FirstCapacity = 0,
                EconomyRemaining = economyCapacity,
                BusinessRemaining = businessCapacity,
                FirstRemaining = 0
            };

            var validation = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(
                flight,
                new ValidationContext(flight),
                validation,
                validateAllProperties: true);

            if (!isValid ||
                flight.DepartureAt <= DateTime.UtcNow ||
                flight.DepartureCountry.Equals(flight.DestinationCountry, StringComparison.OrdinalIgnoreCase) ||
                economyPrice <= 0 ||
                businessPrice <= 0 ||
                firstPrice <= 0)
            {
                errors.AddRange(validation.Select(error => new
                {
                    row = line + 1,
                    field = error.MemberNames.FirstOrDefault() ?? "row",
                    value = "",
                    message = error.ErrorMessage ?? "Invalid value."
                }).Cast<object>());
            }
            else
            {
                imported.Add(flight);
            }
        }

        if (errors.Count > 0)
        {
            return Results.BadRequest(new { imported = 0, errors });
        }

        await repository.AddFlightsAsync(imported, ct);
        return Results.Ok(new { imported = imported.Count, errors = Array.Empty<object>() });
    }
}
