namespace AirportTicketBookingSystem.Contracts;

public sealed record CsvImportError(int Row, string Field, string Value, string Message);

public sealed record CsvImportResult(int Imported, IReadOnlyList<CsvImportError> Errors);
