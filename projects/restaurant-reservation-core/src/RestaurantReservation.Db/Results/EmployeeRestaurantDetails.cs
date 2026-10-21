namespace RestaurantReservation.Db.Results;

public class EmployeeRestaurantDetails
{
    public int EmployeeId { get; set; }
    public int RestaurantId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;

    public string RestaurantName { get; set; } = string.Empty;
    public string RestaurantAddress { get; set; } = string.Empty;
}