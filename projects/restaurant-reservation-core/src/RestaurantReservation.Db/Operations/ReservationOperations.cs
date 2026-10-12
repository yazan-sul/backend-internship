using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db.Models;

namespace RestaurantReservation.Db.Operations
{
    public class ReservationOperations
    {
        private readonly RestaurantReservationDbContext _context;
        public ReservationOperations(RestaurantReservationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Reservation>> GetReservationsByCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(reservation => reservation.CustomerId == customerId)
                .ToListAsync(cancellationToken);
        }
    }
}