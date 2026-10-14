using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Db.Models;
using RestaurantReservation.Db.Results;

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
        public async Task<List<ReservationDetails>> ListReservationFromViewAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Set<ReservationDetails>()
                .OrderBy(reservation => reservation.ReservationId)
                .ToListAsync(cancellationToken);
        }

    }
}