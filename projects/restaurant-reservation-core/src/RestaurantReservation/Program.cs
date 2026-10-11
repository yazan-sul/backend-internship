using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db;

if (args.Length == 0)
{
    Console.WriteLine("RestaurantReservation: local EF Core setup is ready.");
    Console.WriteLine("Start phase 5 in plan.md: write your entities and mappings in RestaurantReservation.Db.");
    Console.WriteLine("To check SQL Server access, configure the environment variable in docs/setup.md and run with --check-connection.");
    return 0;
}

if (args.Length != 1 || args[0] != "--check-connection")
{
    Console.Error.WriteLine("Usage: dotnet run --project src/RestaurantReservation [-- --check-connection]");
    return 1;
}

try
{
    await using var context = new RestaurantReservationDbContext(DatabaseConfiguration.CreateOptions());
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
    var connected = await context.Database.CanConnectAsync(timeout.Token);

    if (!connected)
    {
        Console.Error.WriteLine("Cannot connect to SQL Server. Check the server, database, credentials, and certificate settings in docs/setup.md.");
        return 1;
    }

    Console.WriteLine("SQL Server connection succeeded. No tables were created or changed.");
    return 0;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
catch (Exception exception) when (exception is DbException or ArgumentException or OperationCanceledException)
{
    Console.Error.WriteLine("The connection check failed. Verify the connection string and SQL Server availability using docs/setup.md.");
    return 1;
}
