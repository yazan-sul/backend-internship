using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db;
using RestaurantReservation.Db.Operations;
using RestaurantReservation.Db.Requests;

namespace RestaurantReservation.Demos;

public static class TablesCrudDemo
{
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new RestaurantReservationDbContext(DatabaseConfiguration.CreateOptions());

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var operations = new TablesOperations(context);
        var created = await operations.CreateAsync(new CreateRestaurantTableRequest
        {
            TableID = 1,
            RestaurantId = 1,
            Capacity = 4,
        }, cancellationToken);
        Console.WriteLine($"Created table with ID {created.TableId}.");

        var updated = await operations.UpdateAsync(created.TableId, new CreateRestaurantTableRequest
        {
            TableID = 10,
            RestaurantId = 1,
            Capacity = 6,
        }, cancellationToken);
        if (!updated)
        {
            throw new InvalidOperationException("The demo table was missing during update.");
        }

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

    }
}