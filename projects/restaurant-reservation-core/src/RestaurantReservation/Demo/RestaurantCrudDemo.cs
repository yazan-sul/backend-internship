using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db;
using RestaurantReservation.Db.Operations;
using RestaurantReservation.Db.Requests;

namespace RestaurantReservation.Demos;

public static class RestaurantCrudDemo
{
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new RestaurantReservationDbContext(DatabaseConfiguration.CreateOptions());

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var operations = new RestaurantOperations(context);

        var created = await operations.CreateAsync(new CreateRestaurantRequest
        {
            Name = "Temporary Demo Restaurant",
            Address = "10 Demo Street",
            PhoneNumber = "+970599000000",
            OpeningHours = "09:00–18:00"
        }, cancellationToken);
        Console.WriteLine($"Created restaurant with ID {created.RestaurantId}.");

        var before = await context.Restaurants.AsNoTracking()
            .SingleAsync(restaurant => restaurant.RestaurantId == created.RestaurantId, cancellationToken);
        Console.WriteLine($"Before update: {before.Name}, {before.Address}, {before.PhoneNumber}, {before.OpeningHours}");

        var updated = await operations.UpdateAsync(created.RestaurantId, new CreateRestaurantRequest
        {
            Name = "Updated Demo Restaurant",
            Address = "20 Demo Street",
            PhoneNumber = "+970599000001",
            OpeningHours = "10:00–20:00"
        }, cancellationToken);
        if (!updated)
        {
            throw new InvalidOperationException("The demo restaurant was missing during the update.");
        }

        var after = await context.Restaurants.AsNoTracking()
            .SingleAsync(restaurant => restaurant.RestaurantId == created.RestaurantId, cancellationToken);
        Console.WriteLine($"After update: {after.Name}, {after.Address}, {after.PhoneNumber}, {after.OpeningHours}");

        var deleted = await operations.DeleteAsync(created.RestaurantId, cancellationToken);
        if (!deleted)
        {
            throw new InvalidOperationException("The demo restaurant was missing during deletion.");
        }

        var stillExists = await context.Restaurants.AsNoTracking()
            .AnyAsync(restaurant => restaurant.RestaurantId == created.RestaurantId, cancellationToken);
        if (stillExists)
        {
            throw new InvalidOperationException("The demo restaurant still exists after deletion.");
        }

        await transaction.CommitAsync(cancellationToken);
        Console.WriteLine($"Deleted restaurant {created.RestaurantId}. Confirmed it no longer exists.");
    }
}
