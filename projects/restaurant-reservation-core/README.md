# RestaurantReservation — EF Core exercise

Status: planning only. No application code, database, migrations, or packages have been created in this phase.

Start with [plan.md](plan.md). It breaks the assignment into small phases, each with a completion check and a Git checkpoint. Work on one unchecked task at a time.

| Document | Purpose |
| --- | --- |
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

## Next session

Begin phase 1 in `plan.md`: identify an accessible SQL Server and Windows SSMS installation, then create the empty database in SSMS. Infrastructure access is still unverified. Do not assume the database already exists.
