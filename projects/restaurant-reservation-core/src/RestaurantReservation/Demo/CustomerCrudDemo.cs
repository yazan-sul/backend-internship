using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db;
using RestaurantReservation.Db.Operations;
using RestaurantReservation.Db.Requests;

namespace RestaurantReservation.Demos;

public static class CustomerCrudDemo
{
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var context = new RestaurantReservationDbContext(DatabaseConfiguration.CreateOptions());

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var operations = new CustomerOperations(context);
        var created = await operations.CreateAsync(new CreateCustomerRequest
        {
            FirstName = "bro",
            LastName = "khaled",
            Email = "customer@example.com",
            PhoneNumber = "+9705990002222",

        }, cancellationToken);
        Console.WriteLine($"Created customer with ID {created.CustomerId}.");
        await transaction.CommitAsync(cancellationToken);

    }
}