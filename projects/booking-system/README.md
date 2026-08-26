# Booking System

Airport ticket booking project scaffold using React 19/Vite/Tailwind on port `5173` and ASP.NET Core .NET 10 on port `5080`.

## Run locally

```sh
bun install
bun run dev
```

Or run each side separately:

```sh
dotnet run --project backend/AirportTicketBookingSystem.csproj --urls http://localhost:5080
bun run --cwd frontend dev -- --port 5173
```

Application data is stored in PostgreSQL. The local Compose database is exposed on host port `55433`; the backend uses `Host=db;Port=5432` inside Compose and the connection string in `backend/appsettings.json` for local runs.

To start the complete containerized stack:

```sh
bun run docker:up
```

The database is initialized automatically with the required tables and demo flights.

Useful checks:

```sh
bun run typecheck
bun run build
curl http://localhost:5080/api/health
```
