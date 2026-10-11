# RestaurantReservation — EF Core exercise

Status: local setup is ready for phase 5. The .NET 10 console and library build, EF Core 10.0.12 tooling discovers the empty context, and connection configuration is wired. SQL Server/SSMS access remains unverified. Entities, mappings, migrations, and queries are reserved for you to write.

Start with the [step-by-step coding guide](docs/coding-guide.md). It tells you which file to create, what class or method to write, and how to check each step. Use [plan.md](plan.md) to track phase completion and Git checkpoints.

| Document | Purpose |
| --- | --- |
| [Step-by-step coding guide](docs/coding-guide.md) | Exact files, coding tasks, hints, and checks from your first entity to the final exercise |
| [Implementation plan](plan.md) | Requirement coverage, phases, repository ownership, and completion criteria |
| [Schema guide](docs/schema.md) | Every table and column from the supplied diagram, relationships, and explicit design assumptions |
| [Setup guide](docs/setup.md) | .NET, SQL Server, SSMS on Windows, migration commands, and Git workflow |
| [Test scenarios](docs/verification.md) | Small deterministic seed dataset and expected query results |
| [Original schema image](docs/exercise-schema.png) | The diagram supplied with this exercise |

## Project identity

- Branch and directory: `restaurant-reservation-core` / `projects/restaurant-reservation-core`.
- Console project: `RestaurantReservation`.
- Class library: `RestaurantReservation.Db`.
- Context: `RestaurantReservationDbContext`.
- SQL Server database: `RestaurantReservationCore`.
- Planned stack: .NET 10 and EF Core 10, using the SQL Server provider.

The older `restaurant-reservation` branch contains a PostgreSQL/raw-SQL web exercise. It is not the requested EF Core console exercise. It is retained as a separate project.

Repository guidance was reviewed in `ai_docs/NEW_PROJECT.md`, `TECH_STACK.md`, `CONVENTIONS.md`, and `ARCHITECTURE.md`. This project retains the branch-per-project layout, project isolation, and focused commits. The explicit assignment overrides the generic React/API/PostgreSQL template: create the required console and library projects when coding begins instead of copying an unrelated web stack.

The root landing-page launcher assumes a web project, a Bun package, and PostgreSQL. This console exercise will use direct `dotnet` commands. It has no landing-page integration planned, and requires no root tooling changes.

## Run the prepared project

From the repository root:

```sh
cd projects/restaurant-reservation-core
dotnet tool restore
dotnet restore RestaurantReservation.sln
dotnet build RestaurantReservation.sln
dotnet run --project src/RestaurantReservation
./scripts/ef.sh --version
```

The default console command works without a database. Use `./scripts/ef.sh` for EF commands on this Mac; it handles the installed runtime architecture mismatch.

## Your next task

Start phase 5 with `Models/Restaurant.cs` in [RestaurantReservation.Db](src/RestaurantReservation.Db). Use [schema.md](docs/schema.md) to decide its properties. Then add its mapping under `Configurations/` and wire it into [RestaurantReservationDbContext.cs](src/RestaurantReservation.Db/RestaurantReservationDbContext.cs). The context is intentionally empty. Work through one entity at a time; no exercise implementations have been supplied.

You can write and build the models now. Before applying migrations in phase 6, complete phase 1 using SQL Server and Windows SSMS, set the environment variable described in [setup.md](docs/setup.md), and verify access:

```sh
dotnet run --project src/RestaurantReservation -- --check-connection
```

This check only opens a connection; it does not create a database or tables. No live database connection has been verified yet.
