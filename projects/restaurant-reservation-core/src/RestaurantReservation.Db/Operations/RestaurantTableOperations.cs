using RestaurantReservation.Db.Models;
using RestaurantReservation.Db.Requests;

namespace RestaurantReservation.Db.Operations
{
    public class TablesOperations
    {
        private readonly RestaurantReservationDbContext _context;

        public TablesOperations(RestaurantReservationDbContext context)
        {
            _context = context;
        }
        public async Task<RestaurantTable> CreateAsync(
            CreateRestaurantTableRequest request,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(request);

            ArgumentException.ThrowIfNullOrWhiteSpace(request.TableID.ToString());
            ArgumentException.ThrowIfNullOrWhiteSpace(request.RestaurantId.ToString());

            if (
                request.Capacity.ToString().Length <= 0)
            {
                throw new ArgumentException("Capacity must be a positive integer.");
            }

            var table = new RestaurantTable
            {
                TableId = request.TableID,
                RestaurantId = request.RestaurantId,
                Capacity = request.Capacity,
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync(cancellationToken);

            return table;
        }
        public async Task<bool> UpdateAsync(
            int tableId,
            CreateRestaurantTableRequest request,
            CancellationToken cancellationToken = default
        )
        {

            ArgumentNullException.ThrowIfNull(request);

            if (tableId <= 0)
            {
                throw new ArgumentException("Table ID must be a positive integer.");
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(request.TableID.ToString());
            ArgumentException.ThrowIfNullOrWhiteSpace(request.RestaurantId.ToString());



            if (
                request.Capacity.ToString().Length <= 0)
            {
                throw new ArgumentException("Capacity must be a positive integer.");
            }

            var table = await _context.Tables.FindAsync([tableId], cancellationToken);

            if (table == null)
            {
                return false;
            }

            table.TableId = request.TableID;
            table.RestaurantId = request.RestaurantId;
            table.Capacity = request.Capacity;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<bool> DeleteAsync(int tableId, CancellationToken cancellationToken = default)
        {
            var table = await _context.Tables.FindAsync([tableId], cancellationToken);

            if (table == null)
            {
                return false;
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
