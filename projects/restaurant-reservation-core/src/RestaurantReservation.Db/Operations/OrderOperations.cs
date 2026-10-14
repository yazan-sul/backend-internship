using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db.Models;

namespace RestaurantReservation.Db.Operations
{
    public class OrderOperations
    {
        private readonly RestaurantReservationDbContext _context;

        public OrderOperations(RestaurantReservationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> ListOrdersAndMenuItemsAsync(
            int reservationId,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Orders
                .Where(order => order.ReservationId == reservationId)
                .Include(order => order.OrderItems)
                .ThenInclude(orderMenuItem => orderMenuItem.MenuItem)
                .OrderBy(order => order.OrderId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}