# Restaurant Reservation Management System

## Project plan

Project and branch name: `restaurant-reservation`

```text
projects/restaurant-reservation/
```

## Important interpretation

The assignment says both PostgreSQL and MS SQL. Because the repository architecture explicitly requires PostgreSQL and already uses Npgsql and PostgreSQL 18, the implementation will use PostgreSQL.

The following assignment wording appears to have been copied from another project and will be interpreted as follows:

- "Borrowed Books Report" means a reserved tables report.
- "BorrowerID" means the newly created `OrderId`.
- "Tech-Lib" means this restaurant reservation database.
- The query-plan reference to "Req #15" means five selected complex queries from requirements 1-10.
- The requested misspelled procedure name `sp_ResrvedTablesReport` will be retained for grading compatibility.
- SQL Server procedures and functions will be implemented with PostgreSQL PL/pgSQL equivalents.

## 1. Project initialization

- Create the `restaurant-reservation` branch.
- Copy `projects/_template` into `projects/restaurant-reservation` while preserving this plan.
- Rename `ProjectTemplate.csproj` to `RestaurantReservation.csproj`.
- Rename the root namespace to `RestaurantReservation`.
- Rename Bun packages and other project metadata.
- Rename the database to `restaurant_reservation`.
- Rename Docker containers and the database volume.
- Replace template frontend titles and visible text.
- Preserve the existing ports unless conflicts are found:
  - Frontend: `5173`
  - Backend: `5080`
  - PostgreSQL host port: `55435`

## 2. Application architecture

```text
React/Vite/Tailwind frontend
           |
           | /api/*
           v
ASP.NET Core Minimal API
           |
           | Npgsql/raw SQL
           v
PostgreSQL 18
```

No ORM is needed. Database operations will use parameterized Npgsql commands and SQL scripts.

The frontend will provide:

- A dashboard and restaurant popularity summary.
- Restaurant and menu-item management.
- Customer and reservation management.
- Employee management.
- Orders with their order items.
- Reports for reservations, revenue, popular items, and future bookings.

## 3. Database design

Core tables:

- `restaurants`
- `menu_items`
- `employees`
- `customers`
- `restaurant_tables`
- `reservations`
- `orders`
- `order_items`
- `audit_log`

Relationships:

- Restaurant one-to-many MenuItems.
- Restaurant one-to-many Employees.
- Restaurant one-to-many Tables.
- Restaurant one-to-many Reservations.
- Customer one-to-many Reservations.
- Table one-to-many Reservations.
- Reservation one-to-many Orders.
- Employee one-to-many Orders.
- Order many-to-many MenuItems through OrderItems.

Constraints will include:

- Positive menu prices and order-item quantities.
- Positive table capacity and reservation party size.
- Party size cannot exceed table capacity, enforced during reservation creation.
- Unique customer email addresses.
- Valid employee positions.
- Order employees must belong to the reservation's restaurant.
- Order items must reference menu items from the same restaurant.
- Duplicate table reservations at the same date and time are prevented.
- Monetary values use `numeric(12,2)`.

Seeded order totals will be calculated from their order items so the data remains internally consistent.

## 4. Entity relationship diagram

Create:

```text
docs/erd/restaurant-erd.png
```

The rendered diagram will display attributes, primary keys, foreign keys, relationship names, connectivity, and cardinality.

## 5. SQL organization

```text
database/
|-- 00-create-database.sql
|-- 01-schema.sql
|-- 02-seed.sql
|-- queries/
|   |-- 01-customer-reservations.sql
|   |-- 02-managers.sql
|   |-- 03-reservation-orders-and-items.sql
|   |-- 04-reservation-menu-items.sql
|   |-- 05-employee-average-order.sql
|   |-- 06-reservations-view.sql
|   |-- 07-employees-view.sql
|   |-- 08-multiple-order-reservations-cte.sql
|   |-- 09-restaurant-popularity.sql
|   `-- 10-monthly-popular-menu-items.sql
|-- functions/
|   |-- 11-fn-calculate-revenue.sql
|   `-- 12-fn-calculate-employee-salary.sql
|-- procedures/
|   |-- 13-sp-resrved-tables-report.sql
|   |-- 14-sp-add-new-order.sql
|   `-- 15-future-reservations-temp-table.sql
|-- triggers/
|   `-- 16-reservation-audit-trigger.sql
|-- indexes/
|   `-- 18-performance-indexes.sql
`-- query-plans/
    |-- 17-before-indexes.sql
    |-- 17-before-indexes.md
    |-- 19-after-indexes.sql
    `-- 19-after-indexes.md
```

Docker initialization will run the schema, seed, views, functions, procedures, triggers, and indexes in a deterministic order.

## 6. Seed-data strategy

Generate deterministic fictional data using fixed IDs or a fixed random seed.

| Entity       | Required count |
| ------------ | -------------: |
| Restaurants  |             50 |
| Menu items   |          1,000 |
| Order items  |          1,500 |
| Orders       |            500 |
| Employees    |            100 |
| Reservations |            500 |
| Customers    |            400 |
| Tables       |            100 |

Data rules:

- Create exactly 20 menu items per restaurant.
- Create exactly 2 employees and 2 tables per restaurant.
- Allow customers to have multiple reservations.
- Give some reservations multiple orders for the CTE exercise.
- Leave some reservations without orders.
- Create exactly 1,500 valid order-item records.
- Ensure menu items and employees match each reservation's restaurant.
- Include past, current, and future reservations.
- Include `Manager`, `VIPOrdersWaiter`, `StandardWaiter`, and `AssistantWaiter` employee positions.

A validation SQL script will assert all required counts and detect broken relationships.

## 7. API plan

Minimal API route groups:

```text
/api/health
/api/dashboard

/api/restaurants
/api/restaurants/{id}
/api/restaurants/{id}/menu-items
/api/restaurants/{id}/revenue

/api/customers
/api/customers/{id}/reservations

/api/employees
/api/employees/managers
/api/employees/{id}/salary
/api/employees/{id}/average-order

/api/reservations
/api/reservations/{id}
/api/reservations/{id}/orders
/api/reservations/{id}/menu-items

/api/orders
/api/orders/{id}

/api/reports/reservations
/api/reports/restaurant-popularity
/api/reports/popular-menu-items
/api/reports/reserved-tables
/api/reports/future-reservations
```

Write endpoints will validate inputs and return consistent problem-details errors.

## 8. Frontend plan

The responsive restaurant-operations interface will include:

- Sidebar navigation.
- Summary cards and a restaurant-popularity chart or table.
- Searchable and paginated data tables.
- Restaurant details and menu views.
- A reservation creation form.
- An order creation form with menu-item selection.
- Customer reservation history.
- Reporting screens with restaurant, employee, customer, and date filters.
- Loading, empty, success, and error states.

## 9. Query plans and indexing

Use these five representative queries:

1. Orders and menu items for a reservation.
2. Reservation reporting view.
3. Reservations with two or more orders.
4. Restaurant popularity ranking.
5. Monthly popular menu-item analysis.

For each query:

- Capture `EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)` before indexes.
- Add focused foreign-key, filter, and composite indexes.
- Capture the plan again.
- Document scan types, join strategies, execution time, and buffer changes.

Candidate indexes include:

- `reservations(customer_id)`
- `reservations(restaurant_id, reservation_date)`
- `reservations(table_id, reservation_date)`
- `orders(reservation_id)`
- `orders(employee_id)`
- `orders(order_date)`
- `order_items(order_id)`
- `order_items(item_id)`
- `menu_items(restaurant_id)`
- `employees(restaurant_id, position)`

Only indexes supported by measured plans and access patterns will be retained.

## 10. Commit strategy

Each numbered database requirement will receive its own SQL file and distinct commit, as required.

Suggested sequence:

1. Scaffold and rename the project.
2. Add the ERD and relational design.
3. Add the database schema.
4. Add seed data and validation.
5. Add customer reservations query.
6. Add managers query.
7. Add reservation orders and items query.
8. Add reservation menu items query.
9. Add employee average-order query.
10. Add reservations reporting view.
11. Add employees reporting view.
12. Add multiple-order reservations CTE.
13. Add restaurant popularity aggregation.
14. Add monthly popular-menu-item analysis.
15. Add restaurant revenue function.
16. Add employee salary function.
17. Add reserved tables report procedure.
18. Add new-order procedure.
19. Add future-reservations temp-table procedure.
20. Add reservation audit trigger.
21. Add pre-index query plans.
22. Add performance indexes.
23. Add post-index query plans.
24. Add the backend API.
25. Add the frontend interface.
26. Add tests and documentation.

## 11. Verification

Before completion:

- Recreate the database from an empty Docker volume.
- Confirm all required record counts.
- Run database-integrity checks.
- Test functions, procedures, views, and trigger behavior.
- Verify that the audit log records inserted reservations.
- Compare before-index and after-index query plans.
- Test success and failure cases for API endpoints.
- Run frontend type checking.
- Run the backend build and tests.
- Run the full Docker Compose stack.
- Verify responsive UI behavior.
- Document setup, schema rationale, SQL requirements, query-plan findings, and screenshots in the project README.

This plan follows the repository conventions documented in `ai_docs/NEW_PROJECT.md` and `ai_docs/ARCHITECTURE.md`.
