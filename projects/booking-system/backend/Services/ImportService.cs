using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AirportTicketBookingSystem.Contracts;
using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Persistence;

namespace AirportTicketBookingSystem.Services;

public sealed class ImportService(PostgresRepository repository)
{
    private static readonly string[] Headers =
    [
        "code", "departureCountry", "destinationCountry", "departureAirport", "arrivalAirport",
        "departureAt", "economyPrice", "businessPrice", "firstPrice",
        "economyCapacity", "businessCapacity", "firstCapacity"
    ];

    public async Task<CsvImportResult> ImportFlightsAsync(Stream stream, CancellationToken ct)
    {
        var existingCodes = (await repository.GetFlightsAsync(ct))
            .Select(flight => flight.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var importedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var flights = new List<Flight>();
        var errors = new List<CsvImportError>();
        var records = await CsvParser.ParseAsync(stream, ct);

        if (records.Count == 0)
        {
            return new CsvImportResult(0, [new CsvImportError(1, "file", "", "The CSV file does not contain any rows.")]);
        }

        var start = IsHeader(records[0].Values) ? 1 : 0;
        foreach (var record in records.Skip(start))
        {
            if (record.Values.All(string.IsNullOrWhiteSpace)) continue;

            if (record.ParseError is not null)
            {
                errors.Add(new CsvImportError(record.Row, "row", record.Raw, record.ParseError));
                continue;
            }

            if (record.Values.Count != Headers.Length)
            {
                errors.Add(new CsvImportError(record.Row, "row", record.Raw, $"Expected {Headers.Length} columns in the order: {string.Join(", ", Headers)}."));
                continue;
            }

            var values = record.Values.Select(value => value.Trim()).ToArray();
            var rowErrors = new List<CsvImportError>();
            var code = values[0];
            var departureCountry = values[1];
            var destinationCountry = values[2];
            var departureAirport = values[3];
            var arrivalAirport = values[4];

            Require(rowErrors, record.Row, "code", code, code.Length > 0, "Code is required.");
            Require(rowErrors, record.Row, "departureCountry", departureCountry, departureCountry.Length > 0, "Departure country is required.");
            Require(rowErrors, record.Row, "destinationCountry", destinationCountry, destinationCountry.Length > 0, "Destination country is required.");
            Require(rowErrors, record.Row, "departureAirport", departureAirport, departureAirport.Length > 0, "Departure airport is required.");
            Require(rowErrors, record.Row, "arrivalAirport", arrivalAirport, arrivalAirport.Length > 0, "Arrival airport is required.");

            DateTime departureAt = default;
            if (!DateTime.TryParse(values[5], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out departureAt))
            {
                rowErrors.Add(new CsvImportError(record.Row, "departureAt", values[5], "Use an ISO date/time such as 2026-09-15T14:30:00Z."));
            }

            var economyPrice = ParseDecimal(rowErrors, record.Row, "economyPrice", values[6]);
            var businessPrice = ParseDecimal(rowErrors, record.Row, "businessPrice", values[7]);
            var firstPrice = ParseDecimal(rowErrors, record.Row, "firstPrice", values[8]);
            var economyCapacity = ParseInt(rowErrors, record.Row, "economyCapacity", values[9]);
            var businessCapacity = ParseInt(rowErrors, record.Row, "businessCapacity", values[10]);
            var firstCapacity = ParseInt(rowErrors, record.Row, "firstCapacity", values[11]);

            if (economyPrice <= 0) rowErrors.Add(new CsvImportError(record.Row, "economyPrice", values[6], "Price must be greater than zero."));
            if (businessPrice <= 0) rowErrors.Add(new CsvImportError(record.Row, "businessPrice", values[7], "Price must be greater than zero."));
            if (firstPrice <= 0) rowErrors.Add(new CsvImportError(record.Row, "firstPrice", values[8], "Price must be greater than zero."));
            if (economyCapacity < 0) rowErrors.Add(new CsvImportError(record.Row, "economyCapacity", values[9], "Capacity cannot be negative."));
            if (businessCapacity < 0) rowErrors.Add(new CsvImportError(record.Row, "businessCapacity", values[10], "Capacity cannot be negative."));
            if (firstCapacity < 0) rowErrors.Add(new CsvImportError(record.Row, "firstCapacity", values[11], "Capacity cannot be negative."));
            if (existingCodes.Contains(code) || !importedCodes.Add(code))
                rowErrors.Add(new CsvImportError(record.Row, "code", code, "Flight code must be unique."));

            var flight = new Flight
            {
                Code = code,
                DepartureCountry = departureCountry,
                DestinationCountry = destinationCountry,
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                DepartureAt = departureAt,
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

            var validation = new List<ValidationResult>();
            Validator.TryValidateObject(flight, new ValidationContext(flight), validation, true);
            errors.AddRange(rowErrors);
            errors.AddRange(validation.Select(error => new CsvImportError(
                record.Row,
                error.MemberNames.FirstOrDefault() ?? "row",
                "",
                error.ErrorMessage ?? "Invalid value.")));
            if (rowErrors.Count == 0 && validation.Count == 0) flights.Add(flight);
        }

        if (errors.Count > 0) return new CsvImportResult(0, errors);
        await repository.AddFlightsAsync(flights, ct);
        return new CsvImportResult(flights.Count, []);
    }

    private static bool IsHeader(IReadOnlyList<string> values) =>
        values.Count == Headers.Length && values.Select(value => value.Trim()).SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase);

    private static void Require(List<CsvImportError> errors, int row, string field, string value, bool valid, string message)
    {
        if (!valid) errors.Add(new CsvImportError(row, field, value, message));
    }

    private static decimal ParseDecimal(List<CsvImportError> errors, int row, string field, string value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : AddAndReturn<decimal>(errors, row, field, value, "Use a decimal number such as 250.00.");

    private static int ParseInt(List<CsvImportError> errors, int row, string field, string value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : AddAndReturn<int>(errors, row, field, value, "Use a whole number.");

    private static T AddAndReturn<T>(List<CsvImportError> errors, int row, string field, string value, string message)
    {
        errors.Add(new CsvImportError(row, field, value, message));
        return default!;
    }

    private sealed record CsvRecord(int Row, string Raw, IReadOnlyList<string> Values, string? ParseError);

    private static class CsvParser
    {
        public static async Task<List<CsvRecord>> ParseAsync(Stream stream, CancellationToken ct)
        {
            using var reader = new StreamReader(stream);
            var text = await reader.ReadToEndAsync(ct);
            var records = new List<CsvRecord>();
            var values = new List<string>();
            var field = new System.Text.StringBuilder();
            var raw = new System.Text.StringBuilder();
            var inQuotes = false;
            var row = 1;
            var startRow = 1;
            string? parseError = null;

            void FinishField() { values.Add(field.ToString()); field.Clear(); }
            void FinishRecord()
            {
                FinishField();
                records.Add(new CsvRecord(startRow, raw.ToString(), values.ToArray(), parseError));
                values.Clear(); raw.Clear(); parseError = null; startRow = row;
            }

            for (var index = 0; index < text.Length; index++)
            {
                var character = text[index];
                raw.Append(character);
                if (character == '"')
                {
                    if (inQuotes && index + 1 < text.Length && text[index + 1] == '"')
                    {
                        field.Append('"'); raw.Append(text[++index]);
                    }
                    else inQuotes = !inQuotes;
                }
                else if (character == ',' && !inQuotes) FinishField();
                else if ((character == '\n' || character == '\r') && !inQuotes)
                {
                    if (character == '\r' && index + 1 < text.Length && text[index + 1] == '\n') raw.Append(text[++index]);
                    FinishRecord(); row++;
                }
                else
                {
                    if (character == '\n') row++;
                    field.Append(character);
                }
            }
            if (inQuotes) parseError = "Quoted field was not closed.";
            if (field.Length > 0 || values.Count > 0 || raw.Length > 0) FinishRecord();
            return records;
        }
    }
}
