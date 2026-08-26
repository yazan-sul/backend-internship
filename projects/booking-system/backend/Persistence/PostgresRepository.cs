using AirportTicketBookingSystem.Models;
using Npgsql;

namespace AirportTicketBookingSystem.Persistence;

public sealed class PostgresRepository(NpgsqlDataSource dataSource)
{
    public async Task<List<Flight>> GetFlightsAsync(CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand("SELECT * FROM flights ORDER BY departure_at");
        await using var reader = await command.ExecuteReaderAsync(ct);
        var result = new List<Flight>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(ReadFlight(reader));
        }
        return result;
    }

    public async Task SeedFlightsAsync(IEnumerable<Flight> flights, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        foreach (var f in flights)
        {
            await using var command = new NpgsqlCommand("INSERT INTO flights VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,$13,$14,$15,$16) ON CONFLICT DO NOTHING", transaction.Connection, transaction);
            AddFlight(command, f);
            await command.ExecuteNonQueryAsync(ct);
        }
        await transaction.CommitAsync(ct);
    }

    public async Task<Passenger?> GetPassengerAsync(Guid id, CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand("SELECT id,name,contact_details FROM passengers WHERE id=$1");
        command.Parameters.AddWithValue(id);
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadPassenger(reader) : null;
    }

    public async Task<List<Passenger>> GetPassengersAsync(CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand("SELECT id,name,contact_details FROM passengers");
        await using var reader = await command.ExecuteReaderAsync(ct);
        var result = new List<Passenger>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(ReadPassenger(reader));
        }

        return result;
    }

    public async Task<List<Booking>> GetBookingsAsync(CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand("SELECT id,passenger_id,flight_id,travel_class,final_price,booked_at,status FROM bookings ORDER BY booked_at DESC");
        await using var reader = await command.ExecuteReaderAsync(ct);
        var result = new List<Booking>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(ReadBooking(reader));
        }

        return result;
    }

    public async Task<Passenger> SavePassengerAsync(Passenger passenger, CancellationToken ct = default)
    {
        await using var command = dataSource.CreateCommand("INSERT INTO passengers (id,name,contact_details) VALUES ($1,$2,$3) RETURNING id,name,contact_details");
        command.Parameters.AddWithValue(passenger.Id);
        command.Parameters.AddWithValue(passenger.Name);
        command.Parameters.AddWithValue(passenger.ContactDetails is null ? DBNull.Value : passenger.ContactDetails);
        await using var reader = await command.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return ReadPassenger(reader);
    }

    public async Task<object> CreateBookingAsync(Booking request, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        var column = request.Class switch
        {
            TravelClass.Business => "business_remaining",
            TravelClass.First => "first_remaining",
            _ => "economy_remaining"
        };

        await using (var command = new NpgsqlCommand($"UPDATE flights SET {column}={column}-1 WHERE id=$1 AND {column}>0 RETURNING id", tx.Connection, tx))
        {
            command.Parameters.AddWithValue(request.FlightId);
            if (await command.ExecuteScalarAsync(ct) is null)
            {
                throw new InvalidOperationException("This class is no longer available.");
            }
        }

        await using (var command = new NpgsqlCommand("INSERT INTO bookings VALUES ($1,$2,$3,$4,$5,$6,$7)", tx.Connection, tx))
        {
            command.Parameters.AddWithValue(request.Id);
            command.Parameters.AddWithValue(request.PassengerId);
            command.Parameters.AddWithValue(request.FlightId);
            command.Parameters.AddWithValue(request.Class.ToString());
            command.Parameters.AddWithValue(request.FinalPrice);
            command.Parameters.AddWithValue(request.BookedAt);
            command.Parameters.AddWithValue(request.Status.ToString());
            await command.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
        return request;
    }

    public async Task<Booking?> CancelBookingAsync(Guid id, Guid passengerId, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        await using var command = new NpgsqlCommand("SELECT id,passenger_id,flight_id,travel_class,final_price,booked_at,status FROM bookings WHERE id=$1 AND passenger_id=$2 FOR UPDATE", tx.Connection, tx);
        command.Parameters.AddWithValue(id);
        command.Parameters.AddWithValue(passengerId);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        var booking = ReadBooking(reader);
        await reader.CloseAsync();
        if (booking.Status == BookingStatus.Cancelled)
        {
            return booking;
        }

        var column = booking.Class switch
        {
            TravelClass.Business => "business_remaining",
            TravelClass.First => "first_remaining",
            _ => "economy_remaining"
        };

        await using var update = new NpgsqlCommand($"UPDATE flights SET {column}={column}+1 WHERE id=$1; UPDATE bookings SET status='Cancelled' WHERE id=$2", tx.Connection, tx);
        update.Parameters.AddWithValue(booking.FlightId);
        update.Parameters.AddWithValue(booking.Id);
        await update.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
        booking.Status = BookingStatus.Cancelled;
        return booking;
    }

    public async Task AddFlightsAsync(IEnumerable<Flight> flights, CancellationToken ct = default)
    {
        foreach (var flight in flights)
        {
            await using var command = dataSource.CreateCommand("INSERT INTO flights VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,$13,$14,$15,$16)");
            AddFlight(command, flight);
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    private static void AddFlight(NpgsqlCommand command, Flight flight)
    {
        foreach (var value in new object[]
        {
            flight.Id, flight.Code, flight.DepartureCountry, flight.DestinationCountry,
            flight.DepartureAirport, flight.ArrivalAirport, flight.DepartureAt,
            flight.EconomyPrice, flight.BusinessPrice, flight.FirstPrice,
            flight.EconomyCapacity, flight.BusinessCapacity, flight.FirstCapacity,
            flight.EconomyRemaining, flight.BusinessRemaining, flight.FirstRemaining
        })
        {
            command.Parameters.AddWithValue(value);
        }
    }

    private static Flight ReadFlight(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        Code = reader.GetString(1),
        DepartureCountry = reader.GetString(2),
        DestinationCountry = reader.GetString(3),
        DepartureAirport = reader.GetString(4),
        ArrivalAirport = reader.GetString(5),
        DepartureAt = reader.GetDateTime(6),
        EconomyPrice = reader.GetDecimal(7),
        BusinessPrice = reader.GetDecimal(8),
        FirstPrice = reader.GetDecimal(9),
        EconomyCapacity = reader.GetInt32(10),
        BusinessCapacity = reader.GetInt32(11),
        FirstCapacity = reader.GetInt32(12),
        EconomyRemaining = reader.GetInt32(13),
        BusinessRemaining = reader.GetInt32(14),
        FirstRemaining = reader.GetInt32(15)
    };

    private static Passenger ReadPassenger(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        Name = reader.GetString(1),
        ContactDetails = reader.IsDBNull(2) ? null : reader.GetString(2)
    };

    private static Booking ReadBooking(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        PassengerId = reader.GetGuid(1),
        FlightId = reader.GetGuid(2),
        Class = Enum.Parse<TravelClass>(reader.GetString(3)),
        FinalPrice = reader.GetDecimal(4),
        BookedAt = reader.GetDateTime(5),
        Status = Enum.Parse<BookingStatus>(reader.GetString(6))
    };
}
