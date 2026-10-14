# Your step-by-step EF Core coding guide

Start at step 1. Do one step, build, and check that you understand it before continuing. You write the classes and methods yourself; this guide gives you filenames, tasks, hints, and expected results.

The solution, projects, EF packages, empty context, and connection setup already exist. You do not need to create them again. You can complete steps 1–11 without a running database. Before step 12, complete SQL Server/SSMS setup using [setup.md](setup.md).

All paths below are relative to `projects/restaurant-reservation-core`. Open a terminal there:

```sh
cd projects/restaurant-reservation-core
dotnet build
```

Use [schema.md](schema.md) for exact SQL column names and constraints, [verification.md](verification.md) for sample data and expected results, and [plan.md](../plan.md) for phase checkpoints. Check boxes only after doing the work. Ask for a review at any checkpoint if you want feedback.

## Step 1 — Create your first entity: Restaurant

**Create:** `src/RestaurantReservation.Db/Models/Restaurant.cs`

An entity is a C# object that EF will map to a database row.

- [ ] Use the namespace `RestaurantReservation.Db.Models`.
- [ ] Define a public class named `Restaurant`.
- [ ] Add these public properties with getters and setters:

| Property | C# type | Meaning |
| --- | --- | --- |
| RestaurantId | int | Primary key |
| Name | string | Restaurant name |
| Address | string | Restaurant address |
| PhoneNumber | string | Phone text, including `+` or leading zeroes |
| OpeningHours | string | Display text such as opening and closing hours |

- [ ] Initialize each string property to `string.Empty` so the new class builds without nullable warnings.
- [ ] Run `dotnet build`.

Leave navigation properties for later steps, when their target classes exist. Initializing a string does not validate it; rejecting blank input comes later.

**Done when:** the class builds and you can explain why PhoneNumber is a string.

## Step 2 — Tell EF how Restaurant maps to SQL

**Create:** `src/RestaurantReservation.Db/Configurations/RestaurantConfiguration.cs`

A configuration class describes the SQL table, columns, and rules for an entity.

- [ ] Use namespace `RestaurantReservation.Db.Configurations`.
- [ ] Define a public class named `RestaurantConfiguration` implementing `IEntityTypeConfiguration<Restaurant>`.
- [ ] Add the required `Configure` method accepting `EntityTypeBuilder<Restaurant>`.
- [ ] Import the model namespace, `Microsoft.EntityFrameworkCore`, and `Microsoft.EntityFrameworkCore.Metadata.Builders`.
- [ ] Map the entity to table `Restaurants` in schema `dbo`.
- [ ] Configure `RestaurantId` as the primary key, generated on insertion.
- [ ] Map every property to its snake_case SQL column name from the table below.
- [ ] Make all four text properties required and configure their maximum lengths.

| Property | SQL column | Maximum text length |
| --- | --- | ---: |
| RestaurantId | restaurant_id | — |
| Name | name | 120 |
| Address | address | 250 |
| PhoneNumber | phone_number | 30 |
| OpeningHours | opening_hours | 200 |

**API hints:** `ToTable`, `HasKey`, `Property`, `HasColumnName`, `ValueGeneratedOnAdd`, `IsRequired`, `HasMaxLength`. Write the calls yourself; build after finishing the configuration.

## Step 3 — Register Restaurant in the context

**Edit:** `src/RestaurantReservation.Db/RestaurantReservationDbContext.cs`

The context coordinates EF's model, queries, change tracking, and saves. A `DbSet<Restaurant>` is the entry point for working with restaurants.

- [ ] Import your Models and Configurations namespaces.
- [ ] Add a public `DbSet<Restaurant>` property named `Restaurants`. An expression-bodied getter calling `Set<Restaurant>()` avoids an uninitialized-property warning.
- [ ] Override `OnModelCreating(ModelBuilder modelBuilder)`.
- [ ] Call the base implementation, then register your `RestaurantConfiguration` with `ApplyConfiguration`.
- [ ] Keep the existing options constructor.
- [ ] Run `dotnet build`.

Continue registering each new configuration explicitly in this method. This makes it easy to see which mappings are active. EF supports separate configuration classes and registering them from `OnModelCreating`; see [Microsoft's model configuration guide](https://learn.microsoft.com/en-us/ef/core/modeling/).

**First review checkpoint:** Restaurant.cs, RestaurantConfiguration.cs, and the context compile. No database table has been created yet.

## Step 4 — Add Customer using the same pattern

**Create:**

- `src/RestaurantReservation.Db/Models/Customer.cs`
- `src/RestaurantReservation.Db/Configurations/CustomerConfiguration.cs`

- [ ] Define public class `Customer` in the Models namespace.
- [ ] Add `CustomerId` as int, and `FirstName`, `LastName`, `Email`, and `PhoneNumber` as strings.
- [ ] Initialize the strings to `string.Empty`.
- [ ] Define `CustomerConfiguration` implementing `IEntityTypeConfiguration<Customer>`.
- [ ] Map to `dbo.Customers`; map all columns using schema.md.
- [ ] Set `CustomerId` as the generated primary key.
- [ ] Set required text lengths: FirstName 80, LastName 80, Email 254, PhoneNumber 30.
- [ ] Add a unique index on Email. Set an explicit case-insensitive column collation, such as `Latin1_General_100_CI_AS`.
- [ ] Add `Customers` as a `DbSet<Customer>` in the context and register its configuration.
- [ ] Build.

**Hints:** `HasIndex`, `IsUnique`, `UseCollation`. A unique email index makes duplicate email values fail when saving.

## Step 5 — Add RestaurantTable and your first relationship

**Create:**

- `src/RestaurantReservation.Db/Models/RestaurantTable.cs`
- `src/RestaurantReservation.Db/Configurations/RestaurantTableConfiguration.cs`

- [ ] Define `RestaurantTable` with int properties `TableId`, `RestaurantId`, and `Capacity`.
- [ ] Add a reference navigation named `Restaurant`, of type `Restaurant`.
- [ ] Return to Restaurant.cs and add a collection navigation named `Tables`, of type `ICollection<RestaurantTable>`. Initialize it with an empty list.
- [ ] For required reference navigations, use a non-nullable type initialized with `null!` for this exercise. This suppresses the compiler warning; it does not load a related object or create an instance.
- [ ] Configure table `dbo.Tables`, its three column names, and generated primary key `TableId`.
- [ ] Configure the relationship: a table has one Restaurant; a Restaurant has many Tables; the foreign key is RestaurantId.
- [ ] Set delete behavior to `DeleteBehavior.NoAction`.
- [ ] Add a database check that capacity must be greater than zero.
- [ ] Add the alternate key `(TableId, RestaurantId)` needed by reservations later.
- [ ] Add `Tables` as a `DbSet<RestaurantTable>`, register the configuration, and build.

**Hints:** `HasOne`, `WithMany`, `HasForeignKey`, `OnDelete`, `HasAlternateKey`. Configure checks using the table-builder form of `ToTable` and `HasCheckConstraint`; the SQL expression uses the mapped column name, such as `capacity`.

RestaurantId is the stored identifier; Restaurant is the related C# object. With this alternate key, keep a saved table's RestaurantId unchanged. You can still update its capacity.

**Done when:** you can explain the difference between a foreign-key property and a navigation property.

## Step 6 — Add Employee

**Create:**

- `src/RestaurantReservation.Db/Models/Employee.cs`
- `src/RestaurantReservation.Db/Configurations/EmployeeConfiguration.cs`

- [ ] Add int properties `EmployeeId`, `RestaurantId`.
- [ ] Add string properties `FirstName`, `LastName`, `Position`; initialize them.
- [ ] Add the Employee.Restaurant reference and Restaurant.Employees collection navigations.
- [ ] Map to `dbo.Employees`, configure the generated primary key, and map every column.
- [ ] Set required string lengths: 80, 80, and 50 respectively.
- [ ] Configure the restaurant relationship and `NoAction` deletion.
- [ ] Add `Employees` to the context, register the configuration, and build.

Position is text. You will use `Manager` in the seed data and the first LINQ exercise.

## Step 7 — Add MenuItem

**Create:**

- `src/RestaurantReservation.Db/Models/MenuItem.cs`
- `src/RestaurantReservation.Db/Configurations/MenuItemConfiguration.cs`

- [ ] Add int properties `ItemId`, `RestaurantId`; string `Name`; nullable string `Description`; decimal `Price`.
- [ ] Initialize Name. Description may remain null.
- [ ] Add MenuItem.Restaurant and Restaurant.MenuItems navigations.
- [ ] Map to `dbo.MenuItems` and explicitly configure `ItemId` as the generated key.
- [ ] Map every column. Require Name with length 120; make Description optional with length 1000.
- [ ] Set Price precision to `(18, 2)` and add a nonnegative-price check.
- [ ] Configure the restaurant relationship and `NoAction` deletion.
- [ ] Add `MenuItems` to the context, register the configuration, and build.

**Hint:** `HasPrecision`. Use decimal for the exercise's money values.

## Step 8 — Add Reservation

**Create:**

- `src/RestaurantReservation.Db/Models/Reservation.cs`
- `src/RestaurantReservation.Db/Configurations/ReservationConfiguration.cs`

- [ ] Add int properties `ReservationId`, `CustomerId`, `RestaurantId`, `TableId`, `PartySize`; add DateTime `ReservationDate`.
- [ ] Add reference navigations `Customer`, `Restaurant`, and `Table` (type RestaurantTable).
- [ ] Add a `Reservations` collection to Customer, Restaurant, and RestaurantTable.
- [ ] Map to `dbo.Reservations`, set the generated key, and map every column.
- [ ] Map ReservationDate to `datetime2`; use UTC dates when creating records.
- [ ] Configure the customer and restaurant relationships using their IDs and `NoAction`.
- [ ] Configure the Table navigation using composite FK `(TableId, RestaurantId)` referencing the table's alternate key in the same order. Use `NoAction` here too.
- [ ] Add a positive-party-size check and a unique index on `(TableId, ReservationDate)`.
- [ ] Add `Reservations` to the context, register the configuration, and build.

**Hint:** `HasPrincipalKey` identifies the table's composite key. Configure one relationship for the Table navigation, not separate competing simple and composite relationships. The composite FK ensures the selected table belongs to the reservation's restaurant. See [Microsoft's foreign and principal keys guide](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/foreign-and-principal-keys).

Checking that PartySize fits the table's capacity comes later in library validation.

## Step 9 — Add Order

**Create:**

- `src/RestaurantReservation.Db/Models/Order.cs`
- `src/RestaurantReservation.Db/Configurations/OrderConfiguration.cs`

- [ ] Add int properties `OrderId`, `ReservationId`, `EmployeeId`; DateTime `OrderDate`; decimal `TotalAmount`.
- [ ] Add Order.Reservation and Order.Employee reference navigations.
- [ ] Add an `Orders` collection to Reservation and Employee.
- [ ] Map to `dbo.Orders`, configure the generated key, and map every column.
- [ ] Set OrderDate to `datetime2`, TotalAmount precision to `(18, 2)`, and add a nonnegative-total check.
- [ ] Configure both relationships with `NoAction`.
- [ ] Add `Orders` to the context, register the configuration, and build.

TotalAmount is a recorded amount supplied when creating/updating an order. Do not calculate it automatically from current menu prices.

## Step 10 — Add OrderItem

**Create:**

- `src/RestaurantReservation.Db/Models/OrderItem.cs`
- `src/RestaurantReservation.Db/Configurations/OrderItemConfiguration.cs`

- [ ] Add int properties `OrderItemId`, `OrderId`, `ItemId`, and `Quantity`.
- [ ] Add reference navigations `Order` and `MenuItem`.
- [ ] Add an `OrderItems` collection to Order and MenuItem.
- [ ] Map to `dbo.OrderItems`, configure the generated key, and map every column.
- [ ] Configure both relationships with `NoAction`; explicitly use ItemId for the MenuItem relationship.
- [ ] Add a positive-quantity check.
- [ ] Add `OrderItems` to the context, register the configuration, and build.

OrderItem is the explicit link between orders and menu items. Keep its own ID and Quantity. Do not add a second implicit many-to-many relationship or a unique order/item-pair rule.

## Step 11 — Review the complete model (finish phase 5)

- [ ] Count eight model classes, eight configuration classes, and eight context DbSets.
- [ ] Check all columns against schema.md, including names, lengths, optional Description, and money precision.
- [ ] Check all ten relationships against the navigation table in schema.md.
- [ ] Ensure collection navigations are initialized and each configuration is registered once.
- [ ] Build successfully.
- [ ] With a connection string configured, run the following to make EF construct and validate its model:

```sh
./scripts/ef.sh dbcontext info --project src/RestaurantReservation.Db --startup-project src/RestaurantReservation --context RestaurantReservationDbContext
```

This command checks the context/model; it does not prove live database connectivity. For offline model checks only, you may temporarily set a non-secret placeholder:

```sh
export ConnectionStrings__RestaurantReservationCore='Server=localhost;Database=RestaurantReservationCore;Integrated Security=True;Encrypt=True'
```

Replace that value with your real configuration before database commands. Finish the phase 5 checkpoint in plan.md after reviewing the model.

## Step 12 — Create your first migration (phase 6)

**Generated files:** `src/RestaurantReservation.Db/Migrations/`

- [ ] Complete the SSMS database creation and environment setup in setup.md.
- [ ] Run `dotnet run --project src/RestaurantReservation -- --check-connection`; require a successful connection.
- [ ] Generate the migration:

```sh
./scripts/ef.sh migrations add InitialCreate --project src/RestaurantReservation.Db --startup-project src/RestaurantReservation --context RestaurantReservationDbContext --output-dir Migrations
```

- [ ] Open the generated migration. Read `Up` (apply the change) and `Down` (reverse it).
- [ ] Confirm it creates eight application tables, correct keys/relationships, and no unexpected columns such as RestaurantId1.
- [ ] Generate and read the SQL with `./scripts/ef.sh migrations script`, using the same project/startup/context options above.
- [ ] Apply it with `./scripts/ef.sh database update`, using the same options.
- [ ] Inspect the tables in SSMS. `__EFMigrationsHistory` is EF's own history table in addition to your eight tables.
- [ ] Verify rollback/reapply and invalid FK/check failures in a separate disposable test database as described in verification.md.

If you find a mapping mistake before applying a migration, remove the latest unapplied migration with `migrations remove`, fix the mapping, and regenerate it. See [Microsoft's migration guide](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing).

**Done when:** the database matches your model and you understand what the generated migration does.

## Step 13 — Add the sample data (phase 7)

**Generate a migration named:** `SeedSampleData`

- [ ] Use `migrations add SeedSampleData` with the same project/startup/context options.
- [ ] In its Up method, use `migrationBuilder.InsertData` to insert the exact fixture from verification.md.
- [ ] Insert Restaurants and Customers first, then Tables/Employees/MenuItems, then Reservations, Orders, and OrderItems.
- [ ] Use fixed IDs, fixed dates, and all required fields. Do not use the current time or random values.
- [ ] In Down, use `DeleteData` for those seed IDs in reverse dependency order.
- [ ] Apply the migration and check all eight counts.
- [ ] Run database update again. It should have nothing new to apply, and counts should stay unchanged.

**Expected counts:** 5 restaurants, 5 customers, 5 tables, 6 employees, 6 menu items, 6 reservations, 6 orders, 7 order items. Total recorded order amount: 171.00.

## Step 14 — Write Restaurant CRUD first (start phase 8)

**Create:**

- `src/RestaurantReservation.Db/Operations/RestaurantOperations.cs`
- `src/RestaurantReservation/Demos/RestaurantCrudDemo.cs`

- [ ] Define RestaurantOperations with a constructor accepting RestaurantReservationDbContext; retain it in a private readonly field.
- [ ] Write `CreateAsync`: accept restaurant data and a cancellation token; validate required text; add the restaurant; await SaveChangesAsync; return the saved entity with its generated ID.
- [ ] Write `UpdateAsync`: accept an ID and editable values; load the existing entity asynchronously; return false if missing; validate and change its properties; save and return true.
- [ ] Write `DeleteAsync`: load by ID; return false if missing; remove it; save and return true. Handle an expected dependent-record conflict clearly.
- [ ] Define a public static `RunAsync` in RestaurantCrudDemo to demonstrate these methods on a temporary restaurant.
- [ ] Update Program.cs to support a `--demo-crud` branch before its current unknown-argument check. Call your demo there; preserve the default instructions and `--check-connection` branch.
- [ ] In the demo, create → query back → update → query back → delete → confirm absence.
- [ ] Print the generated ID and before/after values so you can see what happened.

**Hints:** `Add`, `SingleOrDefaultAsync`, `Remove`, `SaveChangesAsync`. Add only schedules an insert; SaveChangesAsync sends the changes. When you load an entity in the same context, changing its properties is enough for EF to track the update.

Use a fresh context for each independent scenario, particularly after an expected failed save. Read setup options through the existing DatabaseConfiguration. Do not edit connection credentials into code.

**Done when:** you can describe when the SQL actually executes and why a missing ID returns false.

## Step 15 — Repeat CRUD for the other seven entities (finish phase 8)

Create each pair below and complete its create/update/delete demo before starting the next pair. All library files go under `src/RestaurantReservation.Db/Operations/`; all demo files go under `src/RestaurantReservation/Demos/`.

| Operations file / class | Demo file / class | Validation to practice |
| --- | --- | --- |
| CustomerOperations | CustomerCrudDemo | Required contact data and unique email |
| RestaurantTableOperations | RestaurantTableCrudDemo | Existing restaurant; positive capacity; safe capacity changes |
| EmployeeOperations | EmployeeCrudDemo | Existing restaurant and required names/position |
| MenuItemOperations | MenuItemCrudDemo | Existing restaurant, required name, nonnegative price |
| ReservationOperations | ReservationCrudDemo | Matching table/restaurant, valid customer, party size within capacity |
| OrderOperations | OrderCrudDemo | Employee and reservation share a restaurant; nonnegative total |
| OrderItemOperations | OrderItemCrudDemo | Menu item and order share a restaurant; positive quantity |

Each filename has the `.cs` extension. Use the same constructor and async method pattern as RestaurantOperations.

- [ ] Add each demo's RunAsync call to the `--demo-crud` flow in dependency order.
- [ ] Use temporary records and delete children before parents. Preserve the baseline fixture.
- [ ] Test missing IDs, invalid inputs, and protected parent deletes as well as successful changes.
- [ ] Validate updates against existing dependents; keep RestaurantTable.RestaurantId immutable with the chosen alternate key.
- [ ] Keep recorded order totals unchanged when menu prices change.

**Done when:** all 24 mutation methods have a visible console demonstration. Detailed failure cases are listed in verification.md.

## Step 16 — Write the five LINQ queries, one at a time (phases 9–13)

**Create:** `src/RestaurantReservation/Demos/QueryDemos.cs`, with a public static RunAsync method.

Add `--demo-queries` to Program.cs and call QueryDemos there. Add each query to its operations class below, immediately call it from the demo, and verify its result before writing the next query.

| Order | Edit this operations class | Method to write | Task | Expected sample result |
| --- | --- | --- | --- | --- |
| 1 | EmployeeOperations | ListManagersAsync | Filter Position equal to Manager | Employee IDs 1, 3 |
| 2 | ReservationOperations | GetReservationsByCustomerAsync | Filter by customer ID | Customer 1 → reservations 1, 2 |
| 3 | OrderOperations | ListOrdersAndMenuItemsAsync | Load orders for a reservation with their lines, quantities, and menu details | Reservation 1 → orders 1, 2 with their lines |
| 4 | MenuItemOperations | ListOrderedMenuItemsAsync | Find unique menu items ordered for a reservation | Reservation 1 → item IDs 1, 2 once each |
| 5 | EmployeeOperations | CalculateAverageOrderAmountAsync | Average recorded totals for an employee | Employee 2 → 27.50; employee 6 → null |

- [ ] Build expressions with Where, Select, OrderBy, and Distinct as needed; execute collection queries with ToListAsync.
- [ ] For order details, practice Include/ThenInclude or a projection. If you choose a projection, create its result classes under `src/RestaurantReservation.Db/Results/`.
- [ ] Preserve an order with no lines as an order with an empty collection.
- [ ] Use a nullable decimal average so an employee with no orders produces null; print “No orders”.
- [ ] Use AsNoTracking for read-only entity results and pass cancellation tokens.
- [ ] Try the missing-ID examples from verification.md.

**Question to answer:** which calls build a query, and which call sends it to SQL Server? Complete each corresponding phase checkpoint separately.

## Step 17 — Add the two views (phase 14)

Complete the reservation view first, then repeat for the employee view.

| Create under the library | Reservation view | Employee view |
| --- | --- | --- |
| Results/ | ReservationDetails.cs | EmployeeRestaurantDetails.cs |
| Configurations/ | ReservationDetailsConfiguration.cs | EmployeeRestaurantDetailsConfiguration.cs |
| Generate migration named | AddReservationDetailsView | AddEmployeeRestaurantDetailsView |
| SQL object | dbo.vw_ReservationDetails | dbo.vw_EmployeeRestaurantDetails |
| Operations class to edit | ReservationOperations | EmployeeOperations |
| Async method | ListReservationsWithDetailsAsync | ListEmployeesWithRestaurantsAsync |

- [ ] Define result properties for the columns described in schema.md's read-model table.
- [ ] Configure each result using HasNoKey and ToView; register the configuration in the context.
- [ ] Generate the migration and write CREATE VIEW SQL in Up using migrationBuilder.Sql; drop the view in Down.
- [ ] Use separate SQL commands for each object and no SSMS GO separators.
- [ ] Apply the migration. The view mapping alone does not create the SQL view.
- [ ] Query through `context.Set<YourResultType>()` and materialize asynchronously.
- [ ] Add calls to QueryDemos. Expect six rows from each view for the unchanged fixture.

## Step 18 — Add the revenue function (phase 15)

**Generate migration:** `AddRestaurantRevenueFunction`

**Create:** `src/RestaurantReservation.Db/Functions/RestaurantDbFunctions.cs`

**Edit:** the context, RestaurantOperations.cs, and QueryDemos.cs.

- [ ] Write SQL in migration Up for `dbo.CalculateRestaurantRevenue`, taking an int restaurant ID and returning decimal(18,2).
- [ ] Sum each order's recorded total once through its reservation. Return zero for no matching orders, including an unknown restaurant ID.
- [ ] Drop the function in Down and apply the migration.
- [ ] Define a static CLR translation stub in RestaurantDbFunctions and map it with HasDbFunction in OnModelCreating. Its body should throw if called directly; EF translates it to SQL.
- [ ] Write CalculateRestaurantRevenueAsync in RestaurantOperations. Execute a query that projects the mapped function for the matching restaurant; return zero when the restaurant itself is absent.
- [ ] Add console calls for restaurant 1, restaurant 5, and ID 999.

**Expected:** 90.00, 0.00, 0.00. The CLR stub is for translation; the async wrapper performs the database query. More mapping detail is linked from schema.md.

## Step 19 — Add the party-size procedure (phase 16)

**Generate migration:** `AddCustomersByMinimumPartySizeProcedure`

**Edit:** CustomerOperations.cs and QueryDemos.cs.

- [ ] Write SQL in Up for `dbo.GetCustomersByMinimumPartySize`, taking an int minimum party size.
- [ ] Return customers having a reservation whose party size is strictly greater than the input. Each customer must appear once; EXISTS is useful here.
- [ ] Return all mapped Customer columns and reject negative input in both the procedure and the wrapper.
- [ ] Drop the procedure in Down and apply the migration.
- [ ] Write GetCustomersByMinimumPartySizeAsync using parameterized FromSql/FromSqlInterpolated.
- [ ] Materialize with ToListAsync before sorting the results in memory. Do not compose a database OrderBy/Where over EXEC; see [Microsoft's SQL query guidance](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries).
- [ ] Add console calls for thresholds 3, 4, 1, 6, and a negative input.

**Expected:** 3 → customer IDs 1,2,3; 4 → 1,3; 1 → all five once each; 6 → empty; negative → validation failure.

## Step 20 — Move your methods into repositories (phase 17)

**Create folder:** `src/RestaurantReservation.Db/Repositories/`

- [ ] Start with RestaurantOperations: move it into RestaurantRepository.cs, rename its class/constructor/namespace, and update console callers.
- [ ] Build and rerun its demos before moving the next class.
- [ ] Repeat for Customer, RestaurantTable, Employee, MenuItem, Reservation, Order, and OrderItem.
- [ ] Retain constructor-injected context and the async methods you already wrote.
- [ ] Check the final method ownership table in plan.md.
- [ ] Remove the old operations files after each successful move. Do not leave two implementations of the same method.

**Done when:** eight concrete repositories contain your existing working methods and all demos still work.

## Step 21 — Verify your finished exercise (phase 18)

**Create:** `tests/RestaurantReservation.IntegrationTests/` as a .NET test project; add it to the solution and reference RestaurantReservation.Db.

- [ ] Configure a separate disposable SQL Server test database using an environment variable.
- [ ] Add tests for the fixture's query results, constraint failures, views, function, procedure, and important CRUD failures.
- [ ] Reset/isolate test data so mutations do not change other tests' expected results. Do not run operations concurrently on one context.
- [ ] Reproduce setup with a fresh SSMS-created exercise database using the instructions you documented.
- [ ] Build, run the test suite, then run every console demo.
- [ ] Confirm every public exercise method has a console call and the fixture is preserved afterward.
- [ ] Record actual results in verification.md and complete the remaining plan checkpoints.

## At the end of each phase

Update the phase checkboxes only after its checks pass, review your diff, and commit/push the phase as described in [setup.md](setup.md). Keep unrelated project files out of the commit.

For your first sitting, aim only for steps 1–3. Bring those three files for review before moving on to Customer.
