# Seed fixture and verification guide

This is a planned fixture, not seeded data yet. Small, fixed records make query results easy to calculate by hand. Seed with a migration using fixed IDs and dates; applying an already-applied migration again must not insert more rows.

## Baseline rows

Use fictional names/contact information such as `customer1@example.test`. Populate every required field from the schema guide. All five restaurants get an address, phone, and opening-hours text; all employees/customers get names. These labels can become their demo names.

| Table | Count | Proposed records |
| --- | ---: | --- |
| Restaurants | 5 | IDs 1–5, named Restaurant 1–5 |
| Customers | 5 | IDs 1–5, unique fictional emails |
| Tables | 5 | IDs 1–5 belong to restaurant with the same ID; capacities 6, 4, 8, 2, 6 respectively |
| Employees | 6 | See employee mapping below |
| MenuItems | 6 | See menu mapping below |
| Reservations | 6 | See reservations below |
| Orders | 6 | See orders below |
| OrderItems | 7 | See lines below |

| Employee ID | Restaurant | Position |
| ---: | ---: | --- |
| 1 | 1 | Manager |
| 2 | 1 | Waiter |
| 3 | 2 | Manager |
| 4 | 3 | Chef |
| 5 | 4 | Waiter |
| 6 | 5 | Waiter |

| Menu item ID | Restaurant | Demo name | Price |
| ---: | ---: | --- | ---: |
| 1 | 1 | Soup | 10.00 |
| 2 | 1 | Pasta | 15.00 |
| 3 | 2 | Grill | 20.00 |
| 4 | 3 | Salad | 12.50 |
| 5 | 4 | Sandwich | 8.00 |
| 6 | 5 | Dessert | 9.00 |

| Reservation ID | Customer | Restaurant / table | UTC start | Party size |
| ---: | ---: | ---: | --- | ---: |
| 1 | 1 | 1 / 1 | 2026-09-01 18:00 | 2 |
| 2 | 1 | 1 / 1 | 2026-09-02 18:00 | 5 |
| 3 | 2 | 2 / 2 | 2026-09-01 18:00 | 4 |
| 4 | 3 | 3 / 3 | 2026-09-01 18:00 | 6 |
| 5 | 4 | 4 / 4 | 2026-09-01 18:00 | 2 |
| 6 | 5 | 5 / 5 | 2026-09-01 18:00 | 3 |

| Order ID | Reservation | Employee | Recorded total | Lines (item × quantity) |
| ---: | ---: | ---: | ---: | --- |
| 1 | 1 | 1 | 35.00 | 1 × 2; 2 × 1 |
| 2 | 1 | 2 | 10.00 | 1 × 1 |
| 3 | 2 | 2 | 45.00 | 2 × 3 |
| 4 | 3 | 3 | 40.00 | 3 × 2 |
| 5 | 4 | 4 | 25.00 | 4 × 2 |
| 6 | 5 | 5 | 16.00 | 5 × 2 |

OrderItems IDs 1–7 follow the lines above from top to bottom. Give each order a fixed time after its reservation start, e.g. 18:10 for the first order and 18:20 for reservation 1's second order. Reservation 6 has no orders; employee 6 has no orders; restaurant 5 therefore has no revenue.

## Query expectations

Compare sets or explicitly order by IDs; database result order is not implicit.

| Exercise method / operation | Input | Expected baseline result |
| --- | --- | --- |
| ListManagers | — | Employee IDs 1, 3 |
| GetReservationsByCustomer | Customer 1 | Reservation IDs 1, 2 |
| GetReservationsByCustomer | Customer 999 | Empty list |
| ListOrdersAndMenuItems | Reservation 1 | Order 1: Soup ×2 and Pasta ×1; Order 2: Soup ×1 |
| ListOrdersAndMenuItems | Reservation 6 or 999 | Empty list |
| ListOrderedMenuItems | Reservation 1 | Soup and Pasta, once each |
| ListOrderedMenuItems | Reservation 6 or 999 | Empty list |
| CalculateAverageOrderAmount | Employee 2 | (10 + 45) / 2 = 27.50 |
| CalculateAverageOrderAmount | Employee 1 | 35.00 |
| CalculateAverageOrderAmount | Employee 6 or 999 | null, printed as “No orders” |
| Reservation details view | All | Six rows with the correct customer and restaurant |
| Employee/restaurant view | All | Six rows; employees 1 and 2 share Restaurant 1 |
| CalculateRestaurantRevenue | Restaurant 1 | 35 + 10 + 45 = 90.00 |
| CalculateRestaurantRevenue | Restaurants 2, 3, 4, 5 | 40.00, 25.00, 16.00, 0.00 respectively |
| CalculateRestaurantRevenue | Restaurant 999 | 0.00 |
| Party-size procedure | Threshold 3 | Customer IDs 1, 2, 3 |
| Party-size procedure | Threshold 4 | Customer IDs 1, 3; size 4 does not qualify |
| Party-size procedure | Threshold 1 | Customer IDs 1–5 once each, although customer 1 has two matches |
| Party-size procedure | Threshold 6 | Empty list |
| Party-size procedure | Negative threshold | Validation error; enforce in wrapper and procedure |

Cross-check: the total of all six order amounts is 171.00, matching the sum of restaurant revenues. Revenue must not join OrderItems in a way that counts Order 1's 35.00 twice.

## CRUD demonstration pattern

For each of the eight entities, demonstrate create → read back → update → read back → delete → confirm absence. Use temporary fixture records beyond the baseline IDs and follow FK dependency order when setting up and cleaning up. Each phase's console demo must preserve the baseline counts for the query examples.

Use a clearly identified development database for console demonstrations. Automated integration tests own a separate disposable SQL Server database. On an expected failure, print the meaningful reason and continue; on an unexpected failure, fail the demo visibly.

Important cases:

- A missing update/delete ID returns `false` without modifying another row.
- Required/blank fields, duplicate email (including case variants), negative price/total, and nonpositive capacity/party size/quantity are rejected.
- Invalid FK IDs and deleting a referenced parent fail without partial writes.
- A reservation assigned to a table from a different restaurant fails at the database FK boundary.
- Party size above table capacity fails library validation; equal capacity succeeds.
- An order with an employee from another restaurant, or a line with a menu item from another restaurant, fails library validation.
- Relevant parent updates cannot invalidate existing dependent records; shrinking a table below an existing party size is rejected.
- Two bookings for the same table and exact start time conflict. No claim is made about overlap detection beyond that.
- An order with no line items is still returned by the detailed-order query with an empty line collection.
- Changing a menu price leaves stored historical order totals and revenue unchanged.

## Migration and database verification

1. Apply all migrations to a separate fresh SQL Server test database and confirm all eight tables, two views, one function, and one procedure exist when their phases are complete.
2. Check all baseline row counts and expected results.
3. Run migration update again; no duplicate seed rows or duplicate database objects appear.
4. Roll back and reapply in the disposable test database to check `Down` ordering and recovery.
5. Run SQL Server integration checks, not only an EF in-memory substitute. Confirm both library validation and database-enforced failures at the appropriate boundary.
6. Confirm all query execution and persistence paths use asynchronous database methods and pass cancellation tokens. Sequential awaits on one context are deliberate.

## Completion log

Record actual checks here as implementation progresses. Expected numbers above are not evidence that tests have run.

| Date | Phase | Check | Actual result |
| --- | --- | --- | --- |
| 2026-09-24 | 0 | Fixture designed and arithmetic reviewed | No database created or runtime tests run |
