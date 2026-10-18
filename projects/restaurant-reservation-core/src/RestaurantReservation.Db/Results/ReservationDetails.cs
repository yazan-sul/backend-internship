namespace RestaurantReservation.Db.Results;

public class ReservationDetails
{
    public int ReservationId { get; set; }
    public int CustomerId { get; set; }
    public int RestaurantId { get; set; }
    public int TableId { get; set; }
    public int PartySize { get; set; }
    public DateTime ReservationDate { get; set; }

    public string CustomerFirstName { get; set; } = string.Empty;
    public string CustomerLastName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;

    public string RestaurantName { get; set; } = string.Empty;
    public string RestaurantAddress { get; set; } = string.Empty;
}
