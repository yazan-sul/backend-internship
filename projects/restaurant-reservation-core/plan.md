# RestaurantReservation implementation plan

## Scope and working agreement

Prepare first, then implement one task at a time. Phase 0 produces documentation only. All coding phases below are pending.

The deliverable is a .NET console application using an EF Core class library and SQL Server. Use the eight entities in the supplied diagram. Implement all 14 numbered requirements, call every public exercise method with sample data, and commit and push at the end of every completed phase.

Use .NET 10 (`net10.0`), already installed locally as SDK `10.0.400`, with aligned stable EF Core 10 packages/tools. This satisfies .NET 5.0+. Record exact package/tool patch versions when installing them. The source assignment does not specify SQL types or optionality; [schema.md](docs/schema.md) clearly separates diagram facts from proposed implementation choices.

No web frontend, API, authentication, audit table, salary function, performance-index exercise, or extra schema columns are required. The older project is a different assignment and is not the implementation baseline.

## Planned structure

Only README, this plan, and `docs/` exist in the preparation phase. Create the following code structure incrementally:

```text
projects/restaurant-reservation-core/
  README.md
  plan.md
  docs/
    schema.md
    setup.md
    verification.md
    exercise-schema.png
  global.json
  .config/dotnet-tools.json
  RestaurantReservation.sln
  src/
    RestaurantReservation/
      RestaurantReservation.csproj
      Program.cs
      Demos/
    RestaurantReservation.Db/
      RestaurantReservation.Db.csproj
      RestaurantReservationDbContext.cs
      RestaurantReservationDbContextFactory.cs
      Models/
      Configurations/
      Migrations/
      Seeding/
      Results/
      Operations/       # temporary exercise methods before requirement 14
      Repositories/     # final home of methods in requirement 14
  tests/
    RestaurantReservation.IntegrationTests/
```

Keep migrations inside the library, including SQL for views, the database function, and the stored procedure. Use SQL Server for integration checks; an in-memory provider cannot validate these features.

## Requirement coverage

| Assignment | Work phase | Evidence |
| --- | --- | --- |
| Before you start: .NET 5+, one task, async LINQ, GitHub | All phases | Target framework; sequential checklist; async database calls; pushed commits |
| 1. Database in SSMS | 1 | `RestaurantReservationCore` visible in SSMS; sanitized setup evidence |
| 2. Console application | 2 | Console project named `RestaurantReservation` builds/runs |
| 3. Library and reference | 3 | `RestaurantReservation.Db` builds and is referenced by console |
| 4. DbContext | 4 | Exact context name and SQL Server configuration in library |
| 5. Models, keys, relationships, constraints, navigation | 5 | Eight mapped entities; schema review |
| 6. Migrations | 6 | Library migrations recreate the schema and reverse cleanly in a disposable database |
| 7. At least five rows per table | 7 | All eight counts pass; rerun does not duplicate seed data |
| 8. Console demonstrations | 8–17, with final audit in 18 | Every CRUD/query/view/function/procedure method called with sample input |
| 9. Create/Update/Delete for every entity | 8 | 24 mutation methods and verified demo results |
| 10.1–10.5. Five LINQ queries | 9–13 respectively | Expected results from the seed fixture |
| 11.1–11.2. Two views | 14 | Migration-created views and EF query results |
| 12. Restaurant revenue function | 15 | Migration SQL plus library method and console call |
| 13. Party-size stored procedure | 16 | Migration SQL plus parameterized library method and console call |
| 14. One repository per entity | 17 | Eight repository classes under library `Repositories/` |

## Phases and checkpoints

Each phase follows the same cycle: understand the concept, complete the first unchecked task, check its result, update documentation, commit a focused change, push, and record the checkpoint. Do not begin another phase until the current phase is verified. A failed push is an unfinished checkpoint, not a reason to discard the local commit.

### Phase 0 — Preparation (this session)

- [x] Inspect both the existing project branch and the new-project guidance.
- [x] Distinguish the PostgreSQL/raw-SQL exercise from this EF Core exercise.
- [x] Prepare schema notes, setup instructions, seed expectations, and this plan.
- [x] Place documents on `restaurant-reservation-core`, review the exact diff, commit, and push.
- Completion: documentation is available on GitHub; coding has not started.
- Commit: `docs: plan restaurant reservation EF Core exercise`.

### Phase 1 — Create the database in SSMS (requirement 1)

- [ ] Confirm a reachable SQL Server instance and a Windows environment with SSMS.
- [ ] Connect in SSMS and create the empty database named `RestaurantReservationCore`.
- [ ] Verify the database name and connectivity; record sanitized evidence in the setup guide.
- [ ] Keep server/login credentials outside Git.
- Completion: an empty database exists; tables will be created by migrations later.
- Commit: `docs: record SQL Server database setup`.

### Phase 2 — Create the console application (requirement 2)

- [ ] Add solution and project-local SDK configuration.
- [ ] Create `src/RestaurantReservation` as a .NET console project.
- [ ] Build and run the default program.
- Completion: the console runs using `net10.0`.
- Commit: `feat: create RestaurantReservation console project`.

### Phase 3 — Add the class library (requirement 3)

- [ ] Create `src/RestaurantReservation.Db` and add both projects to the solution.
- [ ] Add console-to-library project reference.
- [ ] Add aligned EF Core SQL Server/design dependencies and a project-local `dotnet-ef` tool.
- Completion: restore/build succeeds; dependency direction is console → library.
- Commit: `feat: add RestaurantReservation.Db library`.

### Phase 4 — Add the context (requirement 4)

- [ ] Create `RestaurantReservationDbContext` in the library with options passed to its constructor.
- [ ] Configure `UseSqlServer` through the console composition code and a design-time factory.
- [ ] Read the connection string from environment configuration; fail clearly if missing.
- [ ] Verify tooling discovers the context and connects to the intended database.
- Completion: context discovery works without hardcoded credentials.
- Commit: `feat: configure RestaurantReservationDbContext`.

### Phase 5 — Model the eight tables (requirement 5)

- [ ] Add entities in dependency order: Restaurant, Customer, RestaurantTable, Employee, MenuItem, Reservation, Order, OrderItem.
- [ ] For each entity, add properties, mapping, keys, requiredness, lengths, and navigation properties before moving to the next.
- [ ] Configure checks, indexes, and explicit delete behavior described in the schema guide.
- [ ] Review all relationships against the attached diagram, including the table/restaurant consistency rule.
- Completion: model builds, every diagram column is represented, no unexplained schema additions.
- Commit: `feat: model restaurant reservation entities and relationships`.

### Phase 6 — Create and apply the initial migration (requirement 6)

- [ ] Generate `InitialCreate` into the library's `Migrations/` folder.
- [ ] Inspect the generated SQL, especially decimal types, foreign keys, and delete paths.
- [ ] Apply to the empty SSMS-created database and inspect all eight tables in SSMS.
- [ ] Validate invalid foreign keys/check violations and rollback/reapply using a separate disposable test database.
- Completion: migrations own the schema; do not mix `EnsureCreated` with migrations.
- Commit: `feat: add initial restaurant reservation migration`.

### Phase 7 — Seed understandable sample data (requirement 7)

- [ ] Implement deterministic migration-managed seed data matching `docs/verification.md`.
- [ ] Insert parent rows before dependent rows using fixed identifiers and fixed dates.
- [ ] Ensure at least five records in every table and correct order totals.
- [ ] Apply the seed migration twice through normal migration updates; counts remain unchanged.
- Completion: baseline counts and expected amounts pass before adding query methods.
- Commit: `feat: seed restaurant reservation sample data`.

### Phase 8 — Implement entity CRUD (requirements 8–9)

- [ ] Implement and demonstrate Restaurant create/update/delete.
- [ ] Repeat, one entity at a time: Customer, RestaurantTable, Employee, MenuItem, Reservation, Order, OrderItem.
- [ ] Keep database methods in library `Operations/` classes initially so requirement 14 is a visible learning step.
- [ ] Add async existence checks, validation, and `SaveChangesAsync`; pass cancellation tokens.
- [ ] Demonstrate missing IDs, FK-protected deletes, and business-rule failures as well as success.
- Completion: 24 methods work; demo data is cleaned up in dependency order and baseline fixture remains intact.
- Commit: `feat: implement asynchronous entity CRUD and console demos`.

### Phase 9 — ListManagers (requirements 8, 10.1)

- [ ] Query employees with position `Manager`; execute with `ToListAsync` and print sample results.
- Completion: employee IDs 1 and 3 match the baseline fixture.
- Commit: `feat: list restaurant managers asynchronously`.

### Phase 10 — GetReservationsByCustomer (requirements 8, 10.2)

- [ ] Filter reservations by customer ID, order results consistently, and print customer 1's results.
- Completion: reservation IDs 1 and 2; unknown customer returns an empty list.
- Commit: `feat: query reservations by customer`.

### Phase 11 — ListOrdersAndMenuItems (requirements 8, 10.3)

- [ ] Return order information with item details/quantities for a reservation using a projection or eager loading.
- [ ] Preserve orders with no line items; avoid lazy-loading/N+1 queries.
- Completion: reservation 1 includes orders 1 and 2 with their correct lines.
- Commit: `feat: list reservation orders with menu items`.

### Phase 12 — ListOrderedMenuItems (requirements 8, 10.4)

- [ ] Return distinct menu items across all orders for a reservation, using an async query.
- Completion: reservation 1 returns item IDs 1 and 2 once each.
- Commit: `feat: list distinct menu items for a reservation`.

### Phase 13 — CalculateAverageOrderAmount (requirements 8, 10.5)

- [ ] Average stored order totals for one employee using nullable `AverageAsync`.
- [ ] Demonstrate an employee with orders and an employee without orders.
- Completion: employee 2 → `27.50`; employee 6 → `null`, displayed as “No orders”.
- Commit: `feat: calculate average employee order amount`.

### Phase 14 — Query the two database views (requirements 8, 11)

- [ ] Create reservation/customer/restaurant view through a migration and query its keyless EF read model.
- [ ] Then create employee/restaurant view and its keyless EF read model.
- [ ] Keep these read-only models separate from writable entities; demonstrate both in the console.
- Completion: six rows in each view for the baseline; migration `Down` drops the views.
- Commit: `feat: add reservation and employee reporting views`.

### Phase 15 — Restaurant revenue function (requirements 8, 12)

- [ ] Add migration SQL for `dbo.CalculateRestaurantRevenue(@RestaurantId int)` returning `decimal(18,2)`.
- [ ] Sum `Orders.total_amount` through Reservations, counting every order once; return zero when there are no orders.
- [ ] Map the database function with EF Core and wrap a translated query in a public async library method.
- [ ] Demonstrate matching, empty, and missing restaurant cases.
- Completion: restaurant 1 → `90.00`; restaurant 5 → `0.00`; missing ID follows the documented zero behavior. `Down` removes the function.
- Commit: `feat: add restaurant revenue database function`.

### Phase 16 — Party-size stored procedure (requirements 8, 13)

- [ ] Add migration SQL for `dbo.GetCustomersByMinimumPartySize(@MinimumPartySize int)`.
- [ ] Use strict `party_size > @MinimumPartySize` and return each matching customer once, preferably with `EXISTS`.
- [ ] Execute through a parameterized EF SQL query in the library; materialize with `ToListAsync` before client-side ordering.
- [ ] Demonstrate threshold boundaries, no matches, duplicate-customer prevention, and rejected negative input.
- Completion: threshold 3 → customer IDs 1, 2, 3; threshold 4 → 1, 3. `Down` removes the procedure.
- Commit: `feat: add customer party-size stored procedure`.

### Phase 17 — Move methods into repositories (requirements 8, 14)

- [ ] Create one concrete `{EntityName}Repository.cs` per entity in library `Repositories/`.
- [ ] Move one entity's methods at a time from `Operations/`, update console calls, and verify behavior.
- [ ] Use constructor-injected context; remove temporary operations classes when empty.
- [ ] Retain all console demos and async behavior. No generic repository framework is needed.
- Completion: all method owners match the table below; regression checks still pass.
- Commit: `refactor: organize data access into entity repositories`.

### Phase 18 — Complete the demonstration and handoff

- [ ] Audit every requirement and every public exercise method against console calls.
- [ ] Add focused SQL Server integration checks for constraints, queries, views, function, procedure, and important mutation failures.
- [ ] Reproduce from a fresh SSMS-created database using documented restore, migrations, and run commands.
- [ ] Verify source control contains no secrets, generated build output, or unrelated project edits.
- [ ] Record actual results and final commands, then commit and push.
- Completion: another learner can reproduce the complete exercise using README and setup instructions.
- Commit: `test: verify restaurant reservation exercise end to end`.

## Final method ownership

Use idiomatic `Async` suffixes; the console labels should also show the exact exercise names for easy grading. All eight repositories expose `CreateAsync`, `UpdateAsync`, and `DeleteAsync` for their entity.

| Repository | Additional methods |
| --- | --- |
| RestaurantRepository | `CalculateRestaurantRevenueAsync(restaurantId, cancellationToken)` |
| CustomerRepository | `GetCustomersByMinimumPartySizeAsync(minimumPartySize, cancellationToken)` |
| RestaurantTableRepository | No extra required query |
| EmployeeRepository | `ListManagersAsync`, `CalculateAverageOrderAmountAsync`, `ListEmployeesWithRestaurantsAsync` |
| MenuItemRepository | `ListOrderedMenuItemsAsync(reservationId, cancellationToken)` |
| ReservationRepository | `GetReservationsByCustomerAsync`, `ListReservationsWithDetailsAsync` |
| OrderRepository | `ListOrdersAndMenuItemsAsync(reservationId, cancellationToken)` |
| OrderItemRepository | No extra required query |

Mutation contracts: create returns the saved entity/ID; update and delete return `false` for a missing ID. Invalid input or business rules produce a clear validation exception caught by the console. Translate expected FK conflicts into an understandable result; unexpected database failures still surface. Read methods return materialized DTO/entity collections, not a deferred `IQueryable` escaping the context lifetime.

Use `Where`, `Select`, and `OrderBy` to build queries and `ToListAsync`, `SingleOrDefaultAsync`, `AnyAsync`, `AverageAsync`, or `SumAsync` to execute them. There is no `WhereAsync` requirement. Do not use `.Result`, `.Wait()`, or parallel operations on the same context. See [Microsoft async guidance](https://learn.microsoft.com/en-us/ef/core/miscellaneous/async).

## Checkpoint record

Update this table only after verification; never mark future work complete based on the plan alone.

| Phase | Result | Commit / GitHub evidence |
| --- | --- | --- |
| 0 | Complete: documentation checked, committed, and pushed; coding not started | [9897900](https://github.com/yazan-sul/backend-internship/commit/9897900) |
| 1–18 | Not started | — |
