using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Contracts;
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
            throw new InvalidOperationException("This booking is already cancelled.");
        }

        await using (var flightCommand = new NpgsqlCommand("SELECT departure_at FROM flights WHERE id=$1 FOR UPDATE", tx.Connection, tx))
        {
            flightCommand.Parameters.AddWithValue(booking.FlightId);
            var departure = await flightCommand.ExecuteScalarAsync(ct) as DateTime?;
            if (!departure.HasValue || departure.Value <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("A booking cannot be cancelled after the flight has departed.");
            }
        }

        var column = booking.Class switch
        {
            TravelClass.Business => "business_remaining",
            TravelClass.First => "first_remaining",
            _ => "economy_remaining"
        };

        await using (var restore = new NpgsqlCommand($"UPDATE flights SET {column}={column}+1 WHERE id=$1", tx.Connection, tx))
        {
            restore.Parameters.AddWithValue(booking.FlightId);
            await restore.ExecuteNonQueryAsync(ct);
        }

        await using (var update = new NpgsqlCommand("UPDATE bookings SET status='Cancelled' WHERE id=$1", tx.Connection, tx))
        {
            update.Parameters.AddWithValue(booking.Id);
            await update.ExecuteNonQueryAsync(ct);
        }
        await tx.CommitAsync(ct);
        booking.Status = BookingStatus.Cancelled;
        return booking;
    }

    public async Task<Booking?> ModifyBookingAsync(
        Guid id,
        ModifyBookingRequest request,
        CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);

        Booking? booking;
        await using (var command = new NpgsqlCommand(
            "SELECT id,passenger_id,flight_id,travel_class,final_price,booked_at,status FROM bookings WHERE id=$1 AND passenger_id=$2 FOR UPDATE",
            tx.Connection,
            tx))
        {
            command.Parameters.AddWithValue(id);
            command.Parameters.AddWithValue(request.PassengerId);
            await using var reader = await command.ExecuteReaderAsync(ct);
            booking = await reader.ReadAsync(ct) ? ReadBooking(reader) : null;
        }

        if (booking is null)
        {
            return null;
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("A cancelled booking cannot be modified.");
        }

        var flightIds = new[] { booking.FlightId, request.FlightId }
            .Distinct()
            .OrderBy(value => value)
            .ToArray();
        var lockedFlights = new Dictionary<Guid, Flight>();

        foreach (var flightId in flightIds)
        {
            await using var command = new NpgsqlCommand("SELECT * FROM flights WHERE id=$1 FOR UPDATE", tx.Connection, tx);
            command.Parameters.AddWithValue(flightId);
            await using var reader = await command.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                throw new InvalidOperationException("Flight not found.");
            }

            lockedFlights[flightId] = ReadFlight(reader);
        }

        var oldFlight = lockedFlights[booking.FlightId];
        var newFlight = lockedFlights[request.FlightId];
        if (oldFlight.DepartureAt <= DateTime.UtcNow || newFlight.DepartureAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("A booking can only be modified before the flight departs.");
        }

        if (booking.FlightId == request.FlightId && booking.Class == request.Class)
        {
            await tx.CommitAsync(ct);
            return booking;
        }

        if (newFlight.Remaining(request.Class) < 1)
        {
            throw new InvalidOperationException("The selected class is no longer available.");
        }

        if (booking.FlightId != request.FlightId || booking.Class != request.Class)
        {
            var oldColumn = SeatColumn(booking.Class);
            await using var restore = new NpgsqlCommand($"UPDATE flights SET {oldColumn}={oldColumn}+1 WHERE id=$1", tx.Connection, tx);
            restore.Parameters.AddWithValue(booking.FlightId);
            await restore.ExecuteNonQueryAsync(ct);
        }

        var newColumn = SeatColumn(request.Class);
        await using (var reserve = new NpgsqlCommand($"UPDATE flights SET {newColumn}={newColumn}-1 WHERE id=$1 AND {newColumn}>0", tx.Connection, tx))
        {
            reserve.Parameters.AddWithValue(request.FlightId);
            if (await reserve.ExecuteNonQueryAsync(ct) != 1)
            {
                throw new InvalidOperationException("The selected class is no longer available.");
            }
        }

        await using (var update = new NpgsqlCommand(
            "UPDATE bookings SET flight_id=$1,travel_class=$2,final_price=$3 WHERE id=$4",
            tx.Connection,
            tx))
        {
            update.Parameters.AddWithValue(request.FlightId);
            update.Parameters.AddWithValue(request.Class.ToString());
            update.Parameters.AddWithValue(newFlight.Price(request.Class));
            update.Parameters.AddWithValue(booking.Id);
            await update.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
        booking.FlightId = request.FlightId;
        booking.Class = request.Class;
        booking.FinalPrice = newFlight.Price(request.Class);
        return booking;
    }

    public async Task AddFlightsAsync(IEnumerable<Flight> flights, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        foreach (var flight in flights)
        {
            await using var command = new NpgsqlCommand("INSERT INTO flights VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,$13,$14,$15,$16)", connection, transaction);
            AddFlight(command, flight);
            await command.ExecuteNonQueryAsync(ct);
        }
        await transaction.CommitAsync(ct);
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

    private static string SeatColumn(TravelClass travelClass) => travelClass switch
    {
        TravelClass.Business => "business_remaining",
        TravelClass.First => "first_remaining",
        _ => "economy_remaining"
    };

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
