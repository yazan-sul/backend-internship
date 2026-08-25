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

Application data is stored as JSON in `data/`. The directory can be changed with `Booking__DataDirectory` or `Booking:DataDirectory` in `backend/appsettings.json`. Missing files are initialized automatically.

Useful checks:

```sh
bun run typecheck
bun run build
curl http://localhost:5080/api/health
```
