using AirportTicketBookingSystem.Api;
using AirportTicketBookingSystem.Persistence;
using AirportTicketBookingSystem.Services;
using AirportTicketBookingSystem.Validation;
using Npgsql;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("ConnectionStrings:Database is required.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<MigrationRunner>();
builder.Services.AddSingleton<PostgresRepository>();
builder.Services.AddSingleton<BookingService>();
builder.Services.AddSingleton<ImportService>();
builder.Services.AddSingleton<ValidationMetadataProvider>();

var app = builder.Build();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("UnhandledRequestException");
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "The request could not be completed.",
            detail: "An unexpected server error occurred. Try again later.")
            .ExecuteAsync(context);
    });
});
var migrations = app.Services.GetRequiredService<MigrationRunner>();
var repository = app.Services.GetRequiredService<PostgresRepository>();

await migrations.ApplyAsync();

// Seeding is additive. The repository ignores duplicate flight codes, so new
// demo flights are added to existing local databases without touching bookings.
await repository.SeedFlightsAsync(SeedData.Flights());

app
    .MapHealthEndpoints()
    .MapFlightEndpoints()
    .MapPassengerEndpoints()
    .MapBookingEndpoints()
    .MapManagerEndpoints()
    .MapImportEndpoints();

app.Run();
