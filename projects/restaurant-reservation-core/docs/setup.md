# Setup and working guide

The solution, console, library, entity mappings, and design-time factory exist. SDK `10.0.400`, EF Core SQL Server/Design packages `10.0.12`, and local `dotnet-ef` `10.0.12` are installed. The creation commands below are retained as a record; do not rerun them over the existing projects.

## Local SQL Server in Docker

The chosen local learning setup runs SQL Server 2022 Developer in Docker using `linux/amd64` emulation on this Apple Silicon Mac. Microsoft does not officially support this emulation setup. The assignment's separate SSMS requirement still requires Windows; DataGrip can manage the local database but does not satisfy an explicit SSMS requirement by itself.

Run these commands from `projects/restaurant-reservation-core`:

```sh
docker compose up -d sqlserver
docker compose ps
```

The generated, Git-ignored `.env` contains a random local administrator password and `ConnectionStrings__RestaurantReservationCore`. Keep it private. Docker Compose reads `.env` automatically; .NET does not, so use the wrapper below to load it for a command:

```sh
bash scripts/with-local-db.sh dotnet run --project src/RestaurantReservation -- --check-connection
bash scripts/with-local-db.sh ./scripts/ef.sh dbcontext info --project src/RestaurantReservation.Db --startup-project src/RestaurantReservation --context RestaurantReservationDbContext
```

Verified on 2026-10-11: the container is healthy, the empty database exists, the application's actual connection check succeeds, and EF's model check succeeds with `localhost,14333`. The agent's sandbox required additional access for the live connection check. No entity tables or migrations were created by this setup.

To create the empty database after SQL Server is healthy (safe to repeat):

```sh
docker compose exec -T sqlserver bash -c 'SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -b -i /dev/stdin' < scripts/create-local-database.sql
```

For DataGrip, use Microsoft SQL Server, host `localhost`, port `14333`, user `sa`, database `RestaurantReservationCore`, and the password from `.env`. This local-only setup trusts the container's self-signed certificate (`TrustServerCertificate=True`); it binds the port only to this Mac's loopback interface. Use trusted certificates and a dedicated application login for non-local environments.

Database files persist in the Compose volume. `docker compose stop` stops the server; `docker compose up -d sqlserver` starts it again. Do not use `docker compose down -v` unless you intend to erase this project's database.

Use the same `bash scripts/with-local-db.sh` prefix for future EF migration commands. No migrations are applied by the connection or model checks.

## Current observations

Checked on 2026-09-24:

- Repository: `/Users/yazansulaiman/Documents/backend-internship`.
- GitHub remote: `https://github.com/yazan-sul/backend-internship.git`.
- Initial branch: `main`, commit `6670e90`; tracked/untracked Git worktree was clean.
- Existing `restaurant-reservation` branch: commit `72abb04`, with a React/ASP.NET/PostgreSQL/raw-Npgsql exercise and a different `PLAN.md`.
- Mac CPU architecture: `arm64`.
- Installed SDK: .NET `10.0.400`.
- SQL Server access and a Windows SSMS installation have not been verified.

Preparation created and pushed branch `restaurant-reservation-core` from `main` at `6670e90`, after fetching and confirming main matched origin/main. This is now the active branch. Existing ignored build folders from other branches may remain visible; they are not new project source and should not be deleted to make a directory listing look cleaner.

## SQL Server and SSMS on this machine

The assignment explicitly requires database creation using SSMS. SSMS runs on supported Windows systems; it is not a native macOS application. Use a Windows machine or persistent Windows VM with SSMS, connected to an accessible SQL Server. The console application can still be developed and run on the Mac. See [SSMS requirements](https://learn.microsoft.com/en-us/ssms/system-requirements).

Microsoft supports SQL Server Linux containers on Intel/AMD x86-64 Linux hosts, not emulation such as Rosetta/QEMU. A remote supported SQL Server remains an alternative to the local Docker learning setup above. See [SQL Server container support](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/quickstart-install-docker?view=sql-server-ver17).

Before phase 1, establish where Windows/SSMS and SQL Server are available. This is an infrastructure decision, not a reason to substitute PostgreSQL or silently waive the SSMS requirement. No VM or cloud service needs to be purchased as part of this preparation.

### Phase 1 checklist in SSMS

1. Connect to the chosen SQL Server using your supplied credentials.
2. In Object Explorer, open Databases → New Database.
3. Enter `RestaurantReservationCore`, create it, and refresh Databases.
4. Open a query against that database and verify `SELECT DB_NAME();` returns `RestaurantReservationCore`.
5. Leave its application tables empty. EF migrations create them in phase 6.
6. Record a sanitized screenshot or short setup note with server version, database name, and verification date. Keep passwords and connection strings out of evidence.

The application login needs appropriate schema-migration permissions during development and CRUD/query execution afterward. Set its connection string in the environment variable `ConnectionStrings__RestaurantReservationCore`. Both runtime and the design-time factory read this exact variable through `DatabaseConfiguration`. User secrets and `.env` loading are not configured.

Placeholder connection string shape (never commit actual values):

```text
Server=<server>,<port>;Database=RestaurantReservationCore;User ID=<login>;Password=<password>;Encrypt=True;TrustServerCertificate=False;
```

Configure a trusted server certificate for normal use. If a personal development server uses a self-signed certificate, document any local-only certificate exception explicitly rather than hiding it in committed defaults. A `.env` file is not automatically loaded by .NET; use actual environment configuration unless a loader is deliberately added later.

In an interactive zsh terminal, read the actual connection string without placing it in shell history (the prompt hides typing):

```sh
read -rs 'ConnectionStrings__RestaurantReservationCore?SQL Server connection string: '
export ConnectionStrings__RestaurantReservationCore
dotnet run --project src/RestaurantReservation -- --check-connection
```

This sets the value only for the current terminal and its child processes. The check returns exit code 0 for a successful connection and 1 for missing configuration or a failed connection. It does not create tables or apply migrations. The ordinary console command, without `--check-connection`, runs without configuration.

## .NET and EF Core

Installed versions: SDK `10.0.400`; SQL Server provider, Design packages (library and startup console), and local `dotnet-ef` all `10.0.12`. Both Design references use `PrivateAssets=all`.

Use `./scripts/ef.sh` instead of bare `dotnet ef` on this Mac. The tool otherwise selects an installed x64 .NET 8 runtime that cannot load in the ARM64 process. The script sets `DOTNET_ROLL_FORWARD=LatestMajor` for the EF command only, using the working ARM64 .NET 10 runtime. Check with `./scripts/ef.sh --version`.

Use .NET 10 with EF Core 10: EF Core 10 requires .NET 10. See [Microsoft's EF Core 10 release documentation](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew).

The project-local `global.json` pins SDK `10.0.400` with latest-patch roll-forward. The local tool manifest is `.config/dotnet-tools.json`; package references are in each `.csproj`.

### Project-creation reference (already completed)

These commands were run from `projects/restaurant-reservation-core`. The solution explicitly uses `.sln` format.

Phase 2:

```sh
dotnet new sln --name RestaurantReservation --format sln
dotnet new console --name RestaurantReservation --output src/RestaurantReservation --framework net10.0
dotnet sln RestaurantReservation.sln add src/RestaurantReservation/RestaurantReservation.csproj
dotnet run --project src/RestaurantReservation/RestaurantReservation.csproj
```

Phase 3:

```sh
dotnet new classlib --name RestaurantReservation.Db --output src/RestaurantReservation.Db --framework net10.0
dotnet sln RestaurantReservation.sln add src/RestaurantReservation.Db/RestaurantReservation.Db.csproj
dotnet add src/RestaurantReservation/RestaurantReservation.csproj reference src/RestaurantReservation.Db/RestaurantReservation.Db.csproj
dotnet new tool-manifest
```

The EF dependencies and tool were then installed at `10.0.12`; the generated tool manifest was placed under `.config/`. The template `Class1.cs` was removed.

### Future migration commands

Run only after packages, context, models, design-time configuration, and the SSMS-created database are ready. Run from the project directory with the connection-string environment variable set locally.

```sh
dotnet tool restore
./scripts/ef.sh dbcontext info --project src/RestaurantReservation.Db --startup-project src/RestaurantReservation --context RestaurantReservationDbContext
./scripts/ef.sh migrations add InitialCreate --project src/RestaurantReservation.Db --startup-project src/RestaurantReservation --context RestaurantReservationDbContext --output-dir Migrations
./scripts/ef.sh migrations script --project src/RestaurantReservation.Db --startup-project src/RestaurantReservation --context RestaurantReservationDbContext
./scripts/ef.sh database update --project src/RestaurantReservation.Db --startup-project src/RestaurantReservation --context RestaurantReservationDbContext
```

Use the same project/startup/context options for later migrations. Do not use `EnsureCreated` as an alternative to this history. Rollback tests must target a separate, explicitly named disposable test database, never an existing project database.

`dbcontext info` was verified with a non-secret placeholder connection string. It proves context discovery and SQL Server provider configuration, not server connectivity. No migrations have been generated or applied.

### Future daily commands

```sh
dotnet restore RestaurantReservation.sln
dotnet build RestaurantReservation.sln
dotnet run --project src/RestaurantReservation/RestaurantReservation.csproj
```

Once the test project exists, add `dotnet test RestaurantReservation.sln` to verification. Integration tests require a dedicated SQL Server test database; the main exercise database is not a cleanup target.

The root `bun run dev` launcher expects a web project's frontend/backend and PostgreSQL service. Use these direct .NET commands for this console exercise. Do not add a fake frontend, empty Bun package, or project-specific root detector change just to satisfy launcher assumptions.

## Git workflow

Preparation creates the separate `restaurant-reservation-core` branch from the current generic `main`. Inspect/fetch remote state before selecting the base and use the updated main ref if a clean fast-forward is appropriate. Never overwrite the existing `restaurant-reservation` branch or force-push.

For later sessions, verify that the intended branch is active and the worktree is clean before changing tasks. Review the exact diff and stage only this project's intended files. Suggested commands from repository root after completing a phase:

```sh
git status --short
git diff -- projects/restaurant-reservation-core
git add projects/restaurant-reservation-core
git diff --cached --check
git diff --cached --stat
git commit -m "<the phase's meaningful commit message>"
git push
```

On the first push only, establish tracking with `git push -u origin restaurant-reservation-core`. If pushing fails, preserve the local commit and record the authentication/network issue; do not label it pushed. Keep `main` generic and preserve unrelated changes.

Generated `bin/`, `obj/`, editor state, and real secret-bearing configuration must remain ignored. The repository already ignores common build folders and `.env`; verify additional local files before staging.
