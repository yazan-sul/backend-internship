# Booking System

Airport ticket booking project scaffold using React 19/Vite/Tailwind on port `5173` and ASP.NET Core .NET 10 on port `5080`.

## Run locally

```sh
bun install
bun run db:up
bun run dev
```

This starts the project-local PostgreSQL container, then runs the backend on `http://localhost:5080` and frontend on `http://localhost:5173`.

Or run each side separately:

```sh
dotnet run --project backend/AirportTicketBookingSystem.csproj --urls http://localhost:5080
bun run --cwd frontend dev -- --port 5173
```

Run `bun run db:down` when you want to stop the local database container. Application data is stored in PostgreSQL. The local Compose database is exposed on host port `55433`; the backend uses `Host=db;Port=5432` inside Compose and the connection string in `backend/appsettings.json` for local runs.

To start the complete containerized stack:

```sh
bun run docker:up
```

The database, backend, and frontend are initialized and started together. Database migrations and demo flights are applied automatically by the backend. Applied migrations are tracked in the PostgreSQL `schema_migrations` table.

Useful checks:

```sh
bun run typecheck
bun run build
curl http://localhost:5080/api/health
```

The health endpoint reports the backend and PostgreSQL status. The PostgreSQL volume is named `booking-system-data`; remove it only when you intentionally want to reset local application data.
