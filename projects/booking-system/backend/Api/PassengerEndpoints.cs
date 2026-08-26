using AirportTicketBookingSystem.Contracts;
using AirportTicketBookingSystem.Models;
using AirportTicketBookingSystem.Persistence;
using System.ComponentModel.DataAnnotations;

namespace AirportTicketBookingSystem.Api;

public static class PassengerEndpoints
{
    public static IEndpointRouteBuilder MapPassengerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/passengers", async (
            PassengerRequest request,
            PostgresRepository repository,
            CancellationToken ct) =>
        {
            var errors = Validate(request);

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var passenger = new Passenger
            {
                Name = request.Name.Trim(),
                ContactDetails = request.ContactDetails?.Trim()
            };

            return Results.Ok(await repository.SavePassengerAsync(passenger, ct));
        });

        return endpoints;
    }

    private static Dictionary<string, string[]> Validate<T>(T value)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            value!,
            new ValidationContext(value!),
            results,
            validateAllProperties: true);

        return results
            .GroupBy(result => result.MemberNames.FirstOrDefault() ?? "request")
            .ToDictionary(
                group => group.Key,
                group => group.Select(result => result.ErrorMessage ?? "Invalid value.").ToArray());
    }
}
