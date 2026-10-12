using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db.Models;
using RestaurantReservation.Db.Results;

namespace RestaurantReservation.Db.Operations;

public class EmployeeOperations
{
    private readonly RestaurantReservationDbContext _context;

    public EmployeeOperations(RestaurantReservationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> ListManagersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(employee => employee.Position == "Manager")
            .ToListAsync(cancellationToken);
    }
    public async Task<decimal?> CalculateAverageOrderAmountAsync(
    int employeeId,
    CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(order => order.EmployeeId == employeeId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            return null;
        }

        decimal total = 0;
        foreach (var order in orders)
        {
            total += order.TotalAmount;
        }

        return total / orders.Count;

    }

    public async Task<List<EmployeeRestaurantDetails>> ListEmployeesWithRestaurantsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<EmployeeRestaurantDetails>()
            .OrderBy(employee => employee.EmployeeId)
            .ToListAsync(cancellationToken);
    }
}
