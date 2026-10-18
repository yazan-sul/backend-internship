namespace RestaurantReservation.API.Contracts;

public record EmployeeResponse(
    int EmployeeId,
    int RestaurantId,
    string FirstName,
    string LastName,
    string Position);

public record EmployeeAverageOrderAmountResponse(int EmployeeId, decimal? AverageOrderAmount);
