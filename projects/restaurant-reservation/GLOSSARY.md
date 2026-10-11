# Restaurant Reservation Glossary

Use these terms when discussing the restaurant reservation system. The database names in parentheses come from [`database/01-schema.sql`](database/01-schema.sql).

| Term | Meaning |
| --- | --- |
| Restaurant (`restaurants`) | A restaurant that owns dining tables, menu items, and employees. A reservation belongs to one restaurant. |
| Restaurant table (`restaurant_tables`) | A physical dining table at one restaurant. Its `table_number` identifies it within that restaurant; its `capacity` is the number of guests it can seat. |
| Customer (`customers`) | A person who makes a reservation. A customer can make more than one reservation. |
| Reservation (`reservations`) | A customer's booking of one restaurant table at a specific `reservation_date` and time for a `party_size`. It links the customer, restaurant, and table. |
| Reservation date (`reservation_date`) | The date and time of a reservation, stored as a PostgreSQL `timestamptz`. |
| Party size (`party_size`) | The number of guests in a reservation. It must be positive. The project plan also requires it to fit the table's capacity when a reservation is created. |
| Reserved table | A restaurant table linked to a reservation at a specific reservation date and time. The schema prevents two reservations for the same table at the same date and time. |
| Menu item (`menu_items`) | Food or drink offered by one restaurant, with a current price. |
| Employee (`employees`) | A worker at one restaurant. The allowed positions are `Manager`, `VIPOrdersWaiter`, `StandardWaiter`, and `AssistantWaiter`. |
| Order (`orders`) | An order linked to one reservation and one employee. A reservation can have multiple orders or no orders. |
| Order item (`order_items`) | One menu item and its quantity in an order. Its `unit_price` records the price charged when the order was placed. |
| Audit log (`audit_log`) | An append-only record of reservation details written by the reservation audit trigger. |
| Reserved tables report (`sp_ResrvedTablesReport`) | A report of reserved tables in a supplied date range, with reservation, table, and restaurant details. The procedure name keeps the requested spelling for compatibility. |
| Start date (`StartDate`) | The first calendar date included in the reserved tables report. |
| End date (`EndDate`) | The last calendar date included in the reserved tables report. |

## Exercise wording

The original exercise uses some library terms. In this project, read them as follows, as specified in [`PLAN.md`](PLAN.md):

| Exercise wording | Project term |
| --- | --- |
| Borrowed Books Report | Reserved tables report |
| BorrowerID | The newly created `OrderId` |
| Tech-Lib | Restaurant reservation database |
