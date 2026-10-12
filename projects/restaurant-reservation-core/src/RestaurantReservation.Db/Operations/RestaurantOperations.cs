using RestaurantReservation.Db.Models;
using RestaurantReservation.Db.Requests;

namespace RestaurantReservation.Db.Operations
{
    public class RestaurantOperations
    {
        private readonly RestaurantReservationDbContext _context;

        public RestaurantOperations(RestaurantReservationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> UpdateAsync(
            int restaurantId,
            CreateRestaurantRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Address);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.PhoneNumber);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.OpeningHours);

            if (request.Name.Length > 120 || request.Address.Length > 250 ||
                request.PhoneNumber.Length > 30 || request.OpeningHours.Length > 200)
            {
                throw new ArgumentException("One or more parameters exceed the maximum allowed length.");
            }
            var restaurant = await _context.Restaurants.FindAsync([restaurantId], cancellationToken);

            if (restaurant == null)
            {
                return false;
            }

            restaurant.Name = request.Name;
            restaurant.Address = request.Address;
            restaurant.PhoneNumber = request.PhoneNumber;
            restaurant.OpeningHours = request.OpeningHours;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Restaurant> CreateAsync(
            CreateRestaurantRequest request,
            CancellationToken cancellationToken = default)

        {
            ArgumentNullException.ThrowIfNull(request);

            ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Address);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.PhoneNumber);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.OpeningHours);

            if (request.Name.Length > 120 || request.Address.Length > 250 ||
                request.PhoneNumber.Length > 30 || request.OpeningHours.Length > 200)
            {
                throw new ArgumentException("One or more parameters exceed the maximum allowed length.");
            }

            var restaurant = new Restaurant
            {
                Name = request.Name,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                OpeningHours = request.OpeningHours
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync(cancellationToken);
            return restaurant;
        }
        public async Task<bool> DeleteAsync(
            int restaurantId,
            CancellationToken cancellationToken = default
        )
        {
            var restaurant = await _context.Restaurants.FindAsync([restaurantId], cancellationToken);

            if (restaurant == null)
            {
                return false;
            }

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

}
