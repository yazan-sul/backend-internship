using AirportTicketBookingSystem.Domain;
using Npgsql;

namespace AirportTicketBookingSystem.Persistence;

public sealed class PostgresRepository(NpgsqlDataSource dataSource)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand("""
            CREATE TABLE IF NOT EXISTS flights (
                id uuid PRIMARY KEY, code varchar(12) NOT NULL, departure_country varchar(80) NOT NULL,
                destination_country varchar(80) NOT NULL, departure_airport varchar(12) NOT NULL,
                arrival_airport varchar(12) NOT NULL, departure_at timestamptz NOT NULL,
                economy_price numeric(12,2) NOT NULL, business_price numeric(12,2) NOT NULL, first_price numeric(12,2) NOT NULL,
                economy_capacity integer NOT NULL, business_capacity integer NOT NULL, first_capacity integer NOT NULL,
                economy_remaining integer NOT NULL, business_remaining integer NOT NULL, first_remaining integer NOT NULL
            );
            CREATE TABLE IF NOT EXISTS passengers (
                id uuid PRIMARY KEY, name varchar(120) NOT NULL, email varchar(320) NOT NULL UNIQUE, contact_details text NOT NULL
            );
            CREATE TABLE IF NOT EXISTS bookings (
                id uuid PRIMARY KEY, passenger_id uuid NOT NULL REFERENCES passengers(id), flight_id uuid NOT NULL REFERENCES flights(id),
                travel_class varchar(20) NOT NULL, final_price numeric(12,2) NOT NULL, booked_at timestamptz NOT NULL, status varchar(20) NOT NULL
            );
            """);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<List<Flight>> GetFlightsAsync(CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand("SELECT * FROM flights ORDER BY departure_at");
        await using var reader = await command.ExecuteReaderAsync(ct); var result = new List<Flight>();
        while (await reader.ReadAsync(ct)) result.Add(ReadFlight(reader));
        return result;
    }

    public async Task SeedFlightsAsync(IEnumerable<Flight> flights, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        foreach (var f in flights)
        {
            await using var command = new NpgsqlCommand("INSERT INTO flights VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,$13,$14,$15,$16) ON CONFLICT DO NOTHING", transaction.Connection, transaction);
            AddFlight(command, f); await command.ExecuteNonQueryAsync(ct);
        }
        await transaction.CommitAsync(ct);
    }

    public async Task<Passenger?> FindPassengerAsync(string email, CancellationToken ct = default)
    { await using var c = dataSource.CreateCommand("SELECT id,name,email,contact_details FROM passengers WHERE lower(email)=lower($1)"); c.Parameters.AddWithValue(email); await using var r = await c.ExecuteReaderAsync(ct); return await r.ReadAsync(ct) ? ReadPassenger(r) : null; }
    public async Task<List<Passenger>> GetPassengersAsync(CancellationToken ct = default)
    { await using var c = dataSource.CreateCommand("SELECT id,name,email,contact_details FROM passengers"); await using var r = await c.ExecuteReaderAsync(ct); var x = new List<Passenger>(); while (await r.ReadAsync(ct)) x.Add(ReadPassenger(r)); return x; }
    public async Task<List<Booking>> GetBookingsAsync(CancellationToken ct = default)
    { await using var c = dataSource.CreateCommand("SELECT id,passenger_id,flight_id,travel_class,final_price,booked_at,status FROM bookings ORDER BY booked_at DESC"); await using var r = await c.ExecuteReaderAsync(ct); var x = new List<Booking>(); while (await r.ReadAsync(ct)) x.Add(ReadBooking(r)); return x; }

    public async Task<Passenger> SavePassengerAsync(Passenger passenger, CancellationToken ct = default)
    { await using var c = dataSource.CreateCommand("INSERT INTO passengers VALUES ($1,$2,$3,$4) ON CONFLICT (email) DO UPDATE SET name=EXCLUDED.name, contact_details=EXCLUDED.contact_details RETURNING id,name,email,contact_details"); c.Parameters.AddWithValue(passenger.Id); c.Parameters.AddWithValue(passenger.Name); c.Parameters.AddWithValue(passenger.Email); c.Parameters.AddWithValue(passenger.ContactDetails); await using var r = await c.ExecuteReaderAsync(ct); await r.ReadAsync(ct); return ReadPassenger(r); }

    public async Task<object> CreateBookingAsync(Booking request, Passenger passenger, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        await using (var p = new NpgsqlCommand("INSERT INTO passengers VALUES ($1,$2,$3,$4) ON CONFLICT (email) DO UPDATE SET name=EXCLUDED.name, contact_details=EXCLUDED.contact_details", tx.Connection, tx)) { p.Parameters.AddWithValue(passenger.Id); p.Parameters.AddWithValue(passenger.Name); p.Parameters.AddWithValue(passenger.Email); p.Parameters.AddWithValue(passenger.ContactDetails); await p.ExecuteNonQueryAsync(ct); }
        var column = request.Class switch { TravelClass.Business => "business_remaining", TravelClass.First => "first_remaining", _ => "economy_remaining" };
        await using (var f = new NpgsqlCommand($"UPDATE flights SET {column}={column}-1 WHERE id=$1 AND {column}>0 RETURNING id", tx.Connection, tx)) { f.Parameters.AddWithValue(request.FlightId); if (await f.ExecuteScalarAsync(ct) is null) throw new InvalidOperationException("This class is no longer available."); }
        await using (var b = new NpgsqlCommand("INSERT INTO bookings VALUES ($1,$2,$3,$4,$5,$6,$7)", tx.Connection, tx)) { b.Parameters.AddWithValue(request.Id); b.Parameters.AddWithValue(passenger.Id); b.Parameters.AddWithValue(request.FlightId); b.Parameters.AddWithValue(request.Class.ToString()); b.Parameters.AddWithValue(request.FinalPrice); b.Parameters.AddWithValue(request.BookedAt); b.Parameters.AddWithValue(request.Status.ToString()); await b.ExecuteNonQueryAsync(ct); }
        await tx.CommitAsync(ct); return request;
    }

    public async Task<Booking?> CancelBookingAsync(Guid id, string email, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        await using var c = new NpgsqlCommand("SELECT b.id,b.passenger_id,b.flight_id,b.travel_class,b.final_price,b.booked_at,b.status FROM bookings b JOIN passengers p ON p.id=b.passenger_id WHERE b.id=$1 AND lower(p.email)=lower($2) FOR UPDATE", tx.Connection, tx); c.Parameters.AddWithValue(id); c.Parameters.AddWithValue(email); await using var r = await c.ExecuteReaderAsync(ct); if (!await r.ReadAsync(ct)) return null; var booking = ReadBooking(r); await r.CloseAsync(); if (booking.Status == BookingStatus.Cancelled) return booking; var column = booking.Class switch { TravelClass.Business => "business_remaining", TravelClass.First => "first_remaining", _ => "economy_remaining" }; await using var u = new NpgsqlCommand($"UPDATE flights SET {column}={column}+1 WHERE id=$1; UPDATE bookings SET status='Cancelled' WHERE id=$2", tx.Connection, tx); u.Parameters.AddWithValue(booking.FlightId); u.Parameters.AddWithValue(booking.Id); await u.ExecuteNonQueryAsync(ct); await tx.CommitAsync(ct); booking.Status = BookingStatus.Cancelled; return booking;
    }

    public async Task AddFlightsAsync(IEnumerable<Flight> flights, CancellationToken ct = default) { foreach (var f in flights) { await using var c = dataSource.CreateCommand("INSERT INTO flights VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,$13,$14,$15,$16)"); AddFlight(c, f); await c.ExecuteNonQueryAsync(ct); } }

    private static void AddFlight(NpgsqlCommand c, Flight f) { foreach (var v in new object[] { f.Id,f.Code,f.DepartureCountry,f.DestinationCountry,f.DepartureAirport,f.ArrivalAirport,f.DepartureAt,f.EconomyPrice,f.BusinessPrice,f.FirstPrice,f.EconomyCapacity,f.BusinessCapacity,f.FirstCapacity,f.EconomyRemaining,f.BusinessRemaining,f.FirstRemaining }) c.Parameters.AddWithValue(v); }
    private static Flight ReadFlight(NpgsqlDataReader r) => new() { Id=r.GetGuid(0), Code=r.GetString(1), DepartureCountry=r.GetString(2), DestinationCountry=r.GetString(3), DepartureAirport=r.GetString(4), ArrivalAirport=r.GetString(5), DepartureAt=r.GetDateTime(6), EconomyPrice=r.GetDecimal(7), BusinessPrice=r.GetDecimal(8), FirstPrice=r.GetDecimal(9), EconomyCapacity=r.GetInt32(10), BusinessCapacity=r.GetInt32(11), FirstCapacity=r.GetInt32(12), EconomyRemaining=r.GetInt32(13), BusinessRemaining=r.GetInt32(14), FirstRemaining=r.GetInt32(15) };
    private static Passenger ReadPassenger(NpgsqlDataReader r) => new() { Id=r.GetGuid(0), Name=r.GetString(1), Email=r.GetString(2), ContactDetails=r.GetString(3) };
    private static Booking ReadBooking(NpgsqlDataReader r) => new() { Id=r.GetGuid(0), PassengerId=r.GetGuid(1), FlightId=r.GetGuid(2), Class=Enum.Parse<TravelClass>(r.GetString(3)), FinalPrice=r.GetDecimal(4), BookedAt=r.GetDateTime(5), Status=Enum.Parse<BookingStatus>(r.GetString(6)) };
}
