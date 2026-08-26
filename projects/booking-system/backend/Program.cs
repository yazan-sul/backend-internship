using AirportTicketBookingSystem.Api;
using AirportTicketBookingSystem.Persistence;
using AirportTicketBookingSystem.Services;
using AirportTicketBookingSystem.Validation;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("ConnectionStrings:Database is required.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<MigrationRunner>();
builder.Services.AddSingleton<PostgresRepository>();
builder.Services.AddSingleton<BookingService>();
builder.Services.AddSingleton<ValidationMetadataProvider>();

var app = builder.Build();
var migrations = app.Services.GetRequiredService<MigrationRunner>();
var repository = app.Services.GetRequiredService<PostgresRepository>();

await migrations.ApplyAsync();

if (!(await repository.GetFlightsAsync()).Any())
{
    await repository.SeedFlightsAsync(SeedData.Flights());
}

app
    .MapHealthEndpoints()
    .MapFlightEndpoints()
    .MapPassengerEndpoints()
    .MapBookingEndpoints()
    .MapManagerEndpoints()
    .MapImportEndpoints();

app.Run();
