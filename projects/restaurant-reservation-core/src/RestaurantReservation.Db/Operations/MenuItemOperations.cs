using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db.Models;

namespace RestaurantReservation.Db.Operations
{
    public class MenuItemOperations
    {
        private readonly RestaurantReservationDbContext _context;

        public MenuItemOperations(RestaurantReservationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MenuItem>> ListOrderedMenuItemsAsync(
            int reservationId,
            CancellationToken cancellationToken = default
        )
        {

            var orderIems = await _context.OrderItems
                .Where(item => item.Order.ReservationId == reservationId)
                .Include(item => item.MenuItem)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var menuItems = new List<MenuItem>();
            foreach (var orderItem in orderIems)
            {
                bool alreadyAdded = menuItems.Any(menuItem => menuItem.MenuItemId == orderItem.ItemId);
                if (!alreadyAdded)
                {
                    menuItems.Add(orderItem.MenuItem);
                }
            }
            return menuItems
            .OrderByDescending(menuItem => menuItem.Price)
            .ToList();
        }
    }
}