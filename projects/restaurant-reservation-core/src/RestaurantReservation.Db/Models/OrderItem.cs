namespace RestaurantReservation.Db.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }

        public Order Order { get; set; } = null!;
        public MenuItem MenuItem { get; set; } = null!;
    }
}