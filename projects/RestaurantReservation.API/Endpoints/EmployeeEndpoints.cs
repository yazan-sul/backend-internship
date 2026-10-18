using Microsoft.EntityFrameworkCore;
using RestaurantReservation.API.Contracts;
using RestaurantReservation.Db;
using RestaurantReservation.Db.Operations;

namespace RestaurantReservation.API.Endpoints;

public static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/employees").WithTags("Employees");

        group.MapGet("/managers", async (EmployeeOperations operations, CancellationToken ct) =>
        {
            var managers = await operations.ListManagersAsync(ct);
            var response = managers.OrderBy(employee => employee.EmployeeId)
                .Select(employee => new EmployeeResponse(
                    employee.EmployeeId, employee.RestaurantId,
                    employee.FirstName, employee.LastName, employee.Position))
                .ToList();

            return Results.Ok(response);
        }).WithName("ListManagers")
            .WithSummary("List all managers.")
            .Produces<List<EmployeeResponse>>();

        group.MapGet("/{employeeId:int}/average-order-amount", GetAverageOrderAmountAsync)
            .WithName("GetEmployeeAverageOrderAmount")
            .WithSummary("Calculate the average total amount of an employee's orders.")
            .WithDescription("Returns a null average when the employee exists but has no orders.")
            .Produces<EmployeeAverageOrderAmountResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(404);
    }

    private static async Task<IResult> GetAverageOrderAmountAsync(
        int employeeId, RestaurantReservationDbContext db,
        EmployeeOperations operations, CancellationToken ct)
    {
        if (employeeId <= 0)
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["employeeId"] = ["Employee ID must be greater than zero."] });

        if (!await db.Employees.AnyAsync(employee => employee.EmployeeId == employeeId, ct))
            return Results.Problem(statusCode: 404, title: "Employee not found.",
                detail: $"Employee {employeeId} does not exist.");

        var average = await operations.CalculateAverageOrderAmountAsync(employeeId, ct);
        return Results.Ok(new EmployeeAverageOrderAmountResponse(employeeId, average));
    }
}
