using RestaurantReservation.Db;
using RestaurantReservation.Db.Operations;

namespace RestaurantReservation.Demos;

public static class QueryDemos
{
    public static async Task RunAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = new RestaurantReservationDbContext(
            DatabaseConfiguration.CreateOptions());

        // var operations = new EmployeeOperations(context);
        // var managers = await operations.ListManagersAsync(cancellationToken);

        // foreach (var manager in managers)
        // {
        //     Console.WriteLine(
        //         $"{manager.EmployeeId}: {manager.FirstName} {manager.LastName}");
        // }


        // var reservationOperations = new ReservationOperations(context);
        // var reservations = await reservationOperations
        //     .GetReservationsByCustomerAsync(1, cancellationToken);

        // foreach (var reservation in reservations)
        // {
        //     Console.WriteLine(
        //         $"Reservation {reservation.ReservationId}: {reservation.ReservationDate}");
        // }

        // var orderOperations = new OrderOperations(context);
        // var orders = await orderOperations.ListOrdersAndMenuItemsAsync(1, cancellationToken);
        // foreach (var order in orders)
        // {
        //     Console.WriteLine($"Order {order.OrderId}: date {order.OrderDate}");

        //     foreach (var item in order.OrderItems)
        //     {
        //         Console.WriteLine(
        //             $" Item {item.ItemId}:  {item.MenuItem.Name}, quantity {item.Quantity}, price {item.MenuItem.Price}");
        //     }

        //     if (order.OrderItems.Count == 0)
        //     {
        //         Console.WriteLine("  No items.");
        //     }
        // }
        // var MenuItemOperations = new MenuItemOperations(context);
        // var menuItems = await MenuItemOperations.ListOrderedMenuItemsAsync(1, cancellationToken);
        // foreach (var menuItem in menuItems)
        // {
        //     Console.WriteLine($"Menu Item {menuItem.MenuItemId}: {menuItem.Name}, price {menuItem.Price}");
        // }

        var employeeOperations = new EmployeeOperations(context);
        var averageOrderAmount = await employeeOperations.CalculateAverageOrderAmountAsync(45, cancellationToken);
        if (averageOrderAmount.HasValue)
        {
            Console.WriteLine($"Average order amount for employee 1: {averageOrderAmount.Value}");
        }
    }
}