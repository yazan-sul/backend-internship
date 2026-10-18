using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.API.Contracts;
using RestaurantReservation.Db;
using RestaurantReservation.Db.Models;
using RestaurantReservation.Db.Results;

namespace RestaurantReservation.API.Endpoints;

public static class ReservationEndpoints
{
    public static void MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reservations");
        group.MapGet("/", async (RestaurantReservationDbContext db) =>
        {
            var reservations = await db.Reservations
                .AsNoTracking()
                .Select(reservation => new ReservationResponse(
                    reservation.ReservationId,
                    reservation.CustomerId,
                    reservation.RestaurantId,
                    reservation.TableId,
                    reservation.PartySize,
                    DateTime.SpecifyKind(reservation.ReservationDate, DateTimeKind.Utc)))
                .ToListAsync();
            return Results.Ok(reservations);
        });

        group.MapGet("/{id:int}", async (int id, RestaurantReservationDbContext db) =>
        {
            var reservation = await db.Reservations
                .AsNoTracking()
                .Where(reservation => reservation.ReservationId == id)
                .Select(reservation => new ReservationResponse(
                    reservation.ReservationId,
                    reservation.CustomerId,
                    reservation.RestaurantId,
                    reservation.TableId,
                    reservation.PartySize,
                    DateTime.SpecifyKind(reservation.ReservationDate, DateTimeKind.Utc)))
                .SingleOrDefaultAsync();

            if (reservation is null)
                return Results.NotFound();

            return Results.Ok(reservation);
        });

        group.MapGet("/details", async (RestaurantReservationDbContext db) =>
        {
            var details = await db.Set<ReservationDetails>()
                .AsNoTracking()
                .OrderBy(reservation => reservation.ReservationId)
                .ToListAsync();

            return Results.Ok(details);
        });

        group.MapGet("/customer/{customerId:int}", async (int customerId, RestaurantReservationDbContext db) =>
        {
            var reservations = await db.Reservations
                .AsNoTracking()
                .Where(reservation => reservation.CustomerId == customerId)
                .Select(reservation => new ReservationResponse(
                    reservation.ReservationId,
                    reservation.CustomerId,
                    reservation.RestaurantId,
                    reservation.TableId,
                    reservation.PartySize,
                    DateTime.SpecifyKind(reservation.ReservationDate, DateTimeKind.Utc)))
                .ToListAsync();

            return Results.Ok(reservations);
        });
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
    }

    private static async Task<IResult> UpdateAsync(
        int id, ReservationRequest request, RestaurantReservationDbContext db,
        IValidator<ReservationRequest> validator, CancellationToken ct)
    {
        var reservation = await db.Reservations.FindAsync([id], ct);
        if (reservation is null)
            return Results.NotFound();

        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var errors = new Dictionary<string, string[]>();
        if (!await db.Customers.AnyAsync(customer => customer.CustomerId == request.CustomerId, ct))
            errors["customerId"] = ["Customer does not exist."];
        if (!await db.Restaurants.AnyAsync(restaurant => restaurant.RestaurantId == request.RestaurantId, ct))
            errors["restaurantId"] = ["Restaurant does not exist."];

        var table = await db.Tables.AsNoTracking().SingleOrDefaultAsync(
            table => table.TableId == request.TableId && table.RestaurantId == request.RestaurantId, ct);
        if (table is null)
            errors["tableId"] = ["Table does not exist in the specified restaurant."];
        else if (request.PartySize > table.Capacity)
            errors["partySize"] = [$"This table seats up to {table.Capacity} guests."];

        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        reservation.CustomerId = request.CustomerId;
        reservation.RestaurantId = request.RestaurantId;
        reservation.TableId = request.TableId;
        reservation.PartySize = request.PartySize;
        reservation.ReservationDate = request.ReservationDate;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Problem(statusCode: 409, title: "Reservation conflict.",
                detail: "The reservation was deleted by another request.");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        int id, RestaurantReservationDbContext db, CancellationToken ct)
    {
        var reservation = await db.Reservations.FindAsync([id], ct);
        if (reservation is null)
            return Results.NotFound();

        if (await db.Orders.AnyAsync(order => order.ReservationId == id, ct))
            return Results.Problem(statusCode: 409, title: "Reservation conflict.",
                detail: "This reservation has orders and cannot be deleted. Remove its orders first.");

        db.Reservations.Remove(reservation);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Problem(statusCode: 409, title: "Reservation conflict.",
                detail: "The reservation was deleted by another request.");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> CreateAsync(
        ReservationRequest request, RestaurantReservationDbContext db,
        IValidator<ReservationRequest> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var errors = new Dictionary<string, string[]>();
        if (!await db.Customers.AnyAsync(customer => customer.CustomerId == request.CustomerId, ct))
            errors["customerId"] = ["Customer does not exist."];
        if (!await db.Restaurants.AnyAsync(restaurant => restaurant.RestaurantId == request.RestaurantId, ct))
            errors["restaurantId"] = ["Restaurant does not exist."];

        var table = await db.Tables.AsNoTracking().SingleOrDefaultAsync(
            table => table.TableId == request.TableId && table.RestaurantId == request.RestaurantId, ct);
        if (table is null)
            errors["tableId"] = ["Table does not exist in the specified restaurant."];
        else if (request.PartySize > table.Capacity)
            errors["partySize"] = [$"This table seats up to {table.Capacity} guests."];

        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        var reservation = new Reservation
        {
            CustomerId = request.CustomerId,
            RestaurantId = request.RestaurantId,
            TableId = request.TableId,
            PartySize = request.PartySize,
            ReservationDate = request.ReservationDate
        };

        db.Reservations.Add(reservation);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Results.Problem(statusCode: 409, title: "Reservation conflict.",
                detail: "This table is already reserved at that date and time.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            return Results.Problem(statusCode: 409, title: "Reservation conflict.",
                detail: "Related records changed. Refresh the customer, restaurant, and table details and try again.");
        }

        var response = new ReservationResponse(
            reservation.ReservationId,
            reservation.CustomerId,
            reservation.RestaurantId,
            reservation.TableId,
            reservation.PartySize,
            reservation.ReservationDate);

        return Results.Created($"/api/reservations/{reservation.ReservationId}", response);
    }
}
