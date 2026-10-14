namespace RestaurantReservation.Db.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public int RestaurantId { get; set;}
        public string FirstName {get; set; } = string.Empty;
        public string LastName {get; set; } = string.Empty;
        public string Position {get; set; } = string.Empty;

        public Restaurant Restaurant { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}