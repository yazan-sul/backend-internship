namespace RestaurantReservation.Db.Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string Name {get; set; } = string.Empty;
        public string Address {get; set; } = string.Empty;
        public string PhoneNumber {get; set; } = string.Empty;
        public string OpeningHours {get; set; } = string.Empty;
    
        public ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}