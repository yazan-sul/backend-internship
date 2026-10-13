using RestaurantReservation.Db.Models;
using RestaurantReservation.Db.Requests;
using Microsoft.EntityFrameworkCore;


namespace RestaurantReservation.Db.Operations
{
    public class CustomerOperations
    {
        private readonly RestaurantReservationDbContext _context;

        public CustomerOperations(RestaurantReservationDbContext context)
        {
            _context = context;
        }
        public async Task<Customer> CreateAsync(
            CreateCustomerRequest request,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(request);

            ArgumentException.ThrowIfNullOrWhiteSpace(request.FirstName);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.LastName);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.PhoneNumber);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);

            if (request.FirstName.Length > 80 || request.LastName.Length > 80 ||
                request.PhoneNumber.Length > 30 || request.Email.Length > 254)
            {
                throw new ArgumentException("One or more parameters exceed the maximum allowed length.");
            }

            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            return customer;
        }
        public async Task<bool> UpdateAsync(
            int customerId,
            CreateCustomerRequest request,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(request);

            ArgumentException.ThrowIfNullOrWhiteSpace(request.FirstName);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.LastName);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.PhoneNumber);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);

            if (request.FirstName.Length > 80 || request.LastName.Length > 80 ||
                request.PhoneNumber.Length > 30 || request.Email.Length > 254)
            {
                throw new ArgumentException("One or more parameters exceed the maximum allowed length.");
            }

            var customer = await _context.Customers.FindAsync([customerId], cancellationToken);

            if (customer == null)
            {
                return false;
            }

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.PhoneNumber = request.PhoneNumber;
            customer.Email = request.Email;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<bool> DeleteAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var customer = await _context.Customers.FindAsync([customerId], cancellationToken);

            if (customer == null)
            {
                return false;
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<Customer>> GetCustomersByMinimumPartySizeAsync(
            int minimumPartySize,
            CancellationToken cancellationToken = default)
        {
            if (minimumPartySize < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumPartySize),
                    "Minimum party size cannot be negative.");
            }

            var customers = await _context.Customers
                .FromSqlInterpolated(
                    $"EXEC dbo.GetCustomersByMinimumPartySize @MinimumPartySize = {minimumPartySize}")
                .ToListAsync(cancellationToken);

            return customers
                .OrderBy(customer => customer.CustomerId)
                .ToList();
        }
    }
}
