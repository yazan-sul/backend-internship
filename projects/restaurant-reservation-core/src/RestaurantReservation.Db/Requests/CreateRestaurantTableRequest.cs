namespace RestaurantReservation.Db.Requests
{
    public class CreateRestaurantTableRequest
    {
        public int TableID { get; set; }
        public int RestaurantId { get; set; }
        public int Capacity { get; set; }
    }
}