
namespace RestaurantReservation.Db.Models

{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public int RestaurantId { get; set; }
        public string Name {get; set; } = string.Empty;
        public string? Description {get; set; }
        public decimal Price {get; set; }

        public Restaurant Restaurant { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}