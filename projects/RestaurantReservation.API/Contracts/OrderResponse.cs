namespace RestaurantReservation.API.Contracts;

public record MenuItemResponse(
    int MenuItemId,
    int RestaurantId,
    string Name,
    string? Description,
    decimal Price);

public record OrderItemResponse(int OrderItemId, int Quantity, MenuItemResponse MenuItem);

public record OrderResponse(
    int OrderId,
    int ReservationId,
    int EmployeeId,
    DateTime OrderDate,
    decimal TotalAmount,
    List<OrderItemResponse> OrderItems);
