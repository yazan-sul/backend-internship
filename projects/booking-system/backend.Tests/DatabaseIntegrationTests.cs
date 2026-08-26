using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Persistence;
using Npgsql;
using Xunit;

namespace AirportTicketBookingSystem.Tests;

public sealed class DatabaseIntegrationTests
{
    [Fact]
    public async Task Booking_creation_decrements_and_cancellation_restores_a_seat()
    {
        RequireIntegrationEnvironment();

        await using var dataSource = NpgsqlDataSource.Create(ConnectionString());
        var migrations = new MigrationRunner(dataSource);
        await migrations.ApplyAsync();
        var repository = new PostgresRepository(dataSource);
        var flightId = Guid.NewGuid();
        var passengerId = Guid.NewGuid();
        var code = $"P8{Guid.NewGuid():N}"[..12];
        var flight = new Flight
        {
            Id = flightId,
            Code = code,
            DepartureCountry = "Testland",
            DestinationCountry = "Exampleland",
            DepartureAirport = "AAA",
            ArrivalAirport = "BBB",
            DepartureAt = DateTime.UtcNow.AddDays(2),
            EconomyPrice = 100,
            BusinessPrice = 200,
            FirstPrice = 300,
            EconomyCapacity = 2,
            BusinessCapacity = 1,
            FirstCapacity = 1,
            EconomyRemaining = 2,
            BusinessRemaining = 1,
            FirstRemaining = 1
        };

        try
        {
            await repository.AddFlightsAsync([flight]);
            var passenger = await repository.SavePassengerAsync(new Passenger
            {
                Id = passengerId,
                Name = "Phase 8 Test Passenger",
                ContactDetails = "phase8-test@example.test"
            });
            var booking = new Booking
            {
                PassengerId = passenger.Id,
                FlightId = flight.Id,
                Class = TravelClass.Economy,
                FinalPrice = flight.EconomyPrice
            };

            await repository.CreateBookingAsync(booking);
            var afterBooking = Assert.Single(await repository.GetFlightsAsync(), item => item.Id == flightId);
            Assert.Equal(1, afterBooking.EconomyRemaining);

            await repository.CancelBookingAsync(booking.Id, passenger.Id);
            var afterCancellation = Assert.Single(await repository.GetFlightsAsync(), item => item.Id == flightId);
            Assert.Equal(2, afterCancellation.EconomyRemaining);
        }
        finally
        {
            await using (var bookings = dataSource.CreateCommand("DELETE FROM bookings WHERE flight_id=$1"))
            {
                bookings.Parameters.AddWithValue(flightId);
                await bookings.ExecuteNonQueryAsync();
            }

            await using (var passengers = dataSource.CreateCommand("DELETE FROM passengers WHERE id=$1"))
            {
                passengers.Parameters.AddWithValue(passengerId);
                await passengers.ExecuteNonQueryAsync();
            }

            await using (var flights = dataSource.CreateCommand("DELETE FROM flights WHERE id=$1"))
            {
                flights.Parameters.AddWithValue(flightId);
                await flights.ExecuteNonQueryAsync();
            }
        }
    }

    private static void RequireIntegrationEnvironment()
    {
        Assert.True(
            Environment.GetEnvironmentVariable("RUN_DATABASE_INTEGRATION_TESTS") == "1",
            "Set RUN_DATABASE_INTEGRATION_TESTS=1 to run PostgreSQL integration tests.");
    }

    private static string ConnectionString() =>
        Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? "Host=localhost;Port=55433;Database=booking_system;Username=postgres;Password=postgres";
}
