namespace RestaurantReservation.API.Contracts;

public record ReservationRequest(
    int CustomerId,
    int RestaurantId,
    int TableId,
    int PartySize,
    DateTime ReservationDate);

public record ReservationResponse(
    int ReservationId,
    int CustomerId,
    int RestaurantId,
    int TableId,
    int PartySize,
    DateTime ReservationDate);
