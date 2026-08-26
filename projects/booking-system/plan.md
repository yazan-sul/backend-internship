# Airport Ticket Booking System — Full-Stack Implementation Plan

## Goal

Build a maintainable full-stack web application that lets passengers search and book flights, manage their own bookings, and lets a manager filter bookings and import validated flights from CSV files. The React frontend communicates with an ASP.NET Core API, while PostgreSQL provides the application persistence layer.

## Scope and decisions

- Follow `ai_docs/NEW_PROJECT.md`: create the `booking-system` branch first, then copy `projects/_template` into `projects/booking-system/`.
- Keep all project configuration, frontend code, backend code, and database-related project files inside the project directory; do not modify the root project detector.
- Use React 19, TypeScript, Vite, Tailwind CSS, and Zod for the frontend.
- Use ASP.NET Core .NET 10 Minimal API with nullable reference types enabled for the backend.
- Persist application data in PostgreSQL with explicit SQL migrations and transactional updates for booking and seat changes.
- Store imported CSV files separately from application data; never modify the original upload.
- Use `Guid` identifiers for flights, passengers, and bookings.
- Model flight capacity per travel class and track remaining seats so booking availability is enforceable.
- Treat a cancelled booking as historical data with `Cancelled` status rather than deleting it.
- Use DataAnnotations for model constraints and reflection to generate validation metadata for the manager.
- Use a project-local PostgreSQL service in development and Compose; production authentication and managed infrastructure can be added later.
- Use a simple development identity/role selector or seeded demo users for the first version; production authentication can be added later.

### Project setup required by `NEW_PROJECT.md`

```sh
git checkout main
git checkout -b booking-system
cp -R projects/_template projects/booking-system
```

- Rename `ProjectTemplate.csproj` to `AirportTicketBookingSystem.csproj` and update the Dockerfile, backend script, root namespace, and C# namespaces.
- Rename the project package metadata, page titles, and template copy to `booking-system` / `BookingSystem`.
- Keep the frontend on port `5173`, backend on port `5080`, and landing page on port `5174`.
- Adapt the copied Compose and development scripts to run the frontend, backend, and PostgreSQL service together.
- Before each project commit, verify the project branch, run the root tests, build the frontend, and check the local health endpoint.

## Proposed project structure

```text
projects/airport-ticket-booking-system/
├── frontend/
│   ├── package.json
│   ├── vite.config.ts
│   └── src/
│       ├── App.tsx
│       ├── main.tsx
│       ├── index.css
│       ├── components/
│       ├── features/
│       │   ├── flights/
│       │   ├── bookings/
│       │   └── manager/
│       ├── layouts/
│       ├── pages/
│       ├── lib/
│       ├── hooks/
│       └── types/
├── backend/
│   ├── AirportTicketBookingSystem.csproj
│   ├── Program.cs
│   ├── Models/
│   │   ├── Flight.cs
│   │   ├── Passenger.cs
│   │   ├── Booking.cs
│   │   ├── TravelClass.cs
│   │   └── BookingStatus.cs
│   ├── Contracts/
│   │   ├── FlightSearchCriteria.cs
│   │   ├── BookingSearchCriteria.cs
│   │   └── CsvImportResult.cs
│   ├── Validation/
│   │   ├── FlightValidator.cs
│   │   └── ValidationMetadataProvider.cs
│   ├── Persistence/
│   │   ├── PostgresRepository.cs
│   │   └── CsvFlightReader.cs
│   ├── Services/
│   │   ├── FlightService.cs
│   │   ├── BookingService.cs
│   │   ├── ImportService.cs
│   │   └── ValidationDetailsService.cs
│   └── Api/
│       ├── FlightEndpoints.cs
│       ├── BookingEndpoints.cs
│       ├── ManagerEndpoints.cs
│       └── ImportEndpoints.cs
└── README.md
```

## Data model and rules

### Flight

- Flight ID/code, departure country, destination country, departure airport, arrival airport, departure date/time.
- Base economy price and class multipliers/prices for Economy, Business, and First Class.
- Seat capacity and remaining seats for each class.
- Validation: required text fields, valid airport/country values, departure date/time today or later, positive prices, non-negative capacities, and a destination different from the departure location.

### Passenger

- Passenger ID, name, email, and contact details.
- Validate required name/email and normalize the email used to identify the current passenger.

### Booking

- Booking ID, passenger ID, flight ID, selected class, final price, booking timestamp, and status.
- Capture the price at booking time so later flight price changes do not rewrite booking history.
- A booking can be modified only while active and before the flight departs.
- Cancelling restores one seat to the selected class and is idempotent.

## Phases and checkpoints

### Phase 1 — Full-stack scaffold and database foundation

- Copy the generic frontend/backend project scaffold and configure project-local development commands.
- Create the React/Vite frontend shell and ASP.NET Core backend.
- Configure the frontend proxy to the backend API and add a health endpoint.
- Configure the project-local PostgreSQL service and connection settings.
- Add domain enums and entities with nullable reference types enabled.
- Implement the PostgreSQL repository with explicit SQL migrations, explicit mappings, and transactional updates.
- Add sample seed data or a first-run database initialization path.
- Add README instructions for running the frontend, backend, and database, including database connection and reset details.

Checkpoint: the frontend and backend start together, the health check works, and the backend can safely round-trip sample data.

Commit:

```text
feat: create airport booking full-stack foundation
```

### Phase 2 — Flight search API and passenger UI

- Implement a search criteria object with optional filters for price, countries, airports, departure date, and class.
- Add `GET /api/flights` with validated query parameters and explicit response DTOs.
- Apply all supplied filters together with predictable inclusive price/date behavior.
- Calculate class-specific price and availability for each result.
- Build the passenger flight-search page with filter controls, loading state, empty state, validation messages, and results table.
- Validate API responses with Zod and add deterministic date/currency display formatting.

Checkpoint: a passenger can search using any combination of supported parameters and see only available matching flights in the browser.

Commit:

```text
feat: add passenger flight search
```

### Phase 3 — Passenger identity and booking flow

- Add a simple demo identity/role selector for the first version.
- Add `POST /api/passengers` or a seeded-user selection endpoint.
- Implement booking creation from a selected flight and travel class.
- Add `POST /api/bookings` with explicit request/response DTOs.
- Recheck seat availability immediately before saving; decrement the correct class inventory.
- Persist the booking and updated flight as one coordinated operation, with rollback/reload handling if the second write fails.
- Build the booking flow from search results through class selection and confirmation.
- Show booking confirmation with booking ID, flight, class, price, and departure details.

Checkpoint: a passenger can book an available seat in the browser at the correct class price and the seat count persists after restart.

Commit:

```text
feat: allow passengers to book flights
```

### Phase 4 — Passenger booking management UI and API

- Implement `GET /api/bookings/me` for active and historical bookings for the current passenger.
- Implement `PUT /api/bookings/{id}` for eligible modifications.
- Implement `POST /api/bookings/{id}/cancel` for cancellation.
- Build the passenger bookings page with booking table, status badges, cancel confirmation, and modify flow.
- Implement cancellation with confirmation, status update, and seat restoration.
- Implement modification by cancelling/replacing the selected itinerary in a single service-level operation, preserving booking history and applying the new class/flight price.
- Reject modifications and cancellations for invalid IDs, other passengers’ bookings, departed flights, or already-cancelled bookings.

Checkpoint: a passenger can view, cancel, and modify their own eligible bookings in the browser without changing another passenger’s data.

Commit:

```text
feat: add passenger booking management
```

### Phase 5 — Manager dashboard and booking filters

- Implement manager booking search in the service layer with all requested filters.
- Add `GET /api/manager/bookings` with query parameters for flight, price, countries, departure date, airports, passenger, and class.
- Build the manager bookings dashboard with filter controls, results table, booking status, loading, empty, and error states.

Checkpoint: a manager can filter bookings by every requested parameter from the browser and view cancelled bookings distinctly.

Commit:

```text
feat: add manager booking dashboard
```

### Phase 6 — CSV import and model validation

- Define and document the CSV column order and accepted enum/date/decimal formats.
- Implement a CSV reader that handles headers, quoted values, blank rows, malformed column counts, and row numbers.
- Map rows to flight models without crashing on individual bad values.
- Apply model-level validation to every row and return structured errors containing row number, field, rejected value, and message.
- Add cross-field/business validation such as future departure, valid price/capacity, distinct route, unique flight code, and valid class pricing.
- Make imports transactional at the application level: persist no imported flights when the file contains validation errors unless an explicit future partial-import option is added.
- Add `POST /api/manager/flights/import` as a multipart file-upload endpoint.
- Build a manager CSV-upload page with file selection, import summary, and detailed row/field errors.

Checkpoint: the manager can upload a CSV file and receives a complete, actionable error report; valid files import without corrupting existing data.

Commit:

```text
feat: validate and import flights from csv
```

### Phase 7 — Dynamic validation details

- Decorate flight model properties with validation and display metadata.
- Reflect over the flight model to generate field details: display name, CLR/data type, required status, length/range constraints, enum options, and custom rules.
- Add `GET /api/manager/flights/validation-details`.
- Present generated details in the manager UI so documentation stays synchronized with the model.
- Add tests proving that adding or changing a constraint changes the reported metadata.

Checkpoint: the manager can inspect current flight-field constraints directly in the application.

Commit:

```text
feat: expose dynamic flight validation details
```

### Phase 8 — Hardening, tests, and final documentation

- Add unit tests for validation, class pricing, search combinations, seat accounting, booking lifecycle, CSV parsing/import errors, and dynamic metadata.
- Add database-repository tests for schema initialization, transactional writes, seat accounting, and persistence across service instances.
- Test frontend form boundaries, invalid dates/decimals/enums, cancellation/modification edge cases, API errors, and empty data sets.
- Add logging or user-safe error messages for database failures without exposing stack traces in normal use.
- Verify build, tests, clean first-run behavior, restart persistence, and import of both valid and invalid fixtures.
- Update README with CSV schema, sample file, supported validation rules, data location, reset instructions, and known assumptions.

Final commit:

```text
feat: complete airport ticket booking system
```

## Completion criteria

- Passengers can search flights by every requested parameter, select a class, and see class-dependent prices.
- Passengers can create, view, cancel, and modify only their own eligible bookings.
- Seat availability is enforced per class and remains correct after application restarts.
- Managers can filter bookings by every requested parameter.
- Managers can import CSV flights and receive detailed row/field validation errors.
- Validation details are generated dynamically from the flight model’s metadata and custom rules.
- Data is persisted in PostgreSQL and normal booking operations preserve consistency when a transaction fails.
- The code is split into frontend features, API endpoints, domain, persistence, services, and validation responsibilities with focused tests and documentation.

## Git workflow

Create the project branch before implementing project files:

```sh
git checkout main
git checkout -b booking-system
```

After each checkpoint, inspect `git diff`, run the relevant tests, and use a focused Conventional Commit. Keep generated data and local files out of commits unless they are intentional fixtures.

## Progress tracking — 2026-08-26

### Verified repository state

- Branch: `booking-system`.
- Working tree: clean.
- Frontend typecheck: passing.
- Full project build: passing, with existing NuGet vulnerability-feed and nullable warnings.
- Root test suite: passing — 6 tests, 0 failures.
- PostgreSQL container: running and healthy.
- Backend API smoke test: not reachable on port `5080` when the complete development stack is not running.

### Phase status

| Phase                                     | Status          | Notes                                                                                                                                                             |
| ----------------------------------------- | --------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Phase 1 — Foundation and database storage | Mostly complete | Scaffold, PostgreSQL repository, schema initialization, seed data, and Compose integration exist; focused repository tests and final runtime verification remain. |
| Phase 2 — Flight search                   | Partial         | Basic search API and UI exist; query validation and complete filter controls are missing.                                                                         |
| Phase 3 — Passenger booking               | Partial         | Booking creation and transactional PostgreSQL seat decrement exist; the flow and remaining passenger experience are incomplete.                                   |
| Phase 4 — Booking management              | Partial         | Listing and cancellation endpoints exist; modification and management UI are missing.                                                                             |
| Phase 5 — Manager filters                 | Not implemented | Manager currently receives an unfiltered booking list.                                                                                                            |
| Phase 6 — CSV import                      | Partial         | Basic backend import exists; robust CSV parsing, complete validation, and upload UI are missing.                                                                  |
| Phase 7 — Dynamic validation              | Partial         | Basic metadata endpoint exists; custom rules, UI, and tests are missing.                                                                                          |
| Phase 8 — Hardening and documentation     | Not implemented | No project-specific automated test coverage or final documentation pass exists.                                                                                   |

### Current implementation

Already present:

- React/Vite/Tailwind frontend scaffold.
- ASP.NET Core backend scaffold.
- Basic flight search with country, airport, date, price, and class filters.
- Class-specific flight prices and seat availability.
- Passenger creation by email.
- Booking creation and class-specific seat decrement.
- Passenger booking lookup.
- Booking cancellation endpoint.
- Basic manager booking listing.
- Basic CSV import endpoint.
- Basic validation metadata endpoint.
- Successful frontend and backend builds.

### Architecture decision recorded

The project originally described JSON file persistence, but that approach was replaced with PostgreSQL to resolve the persistence and transaction requirements. The current implementation uses:

- `PostgresRepository` as the active persistence layer.
- `Npgsql` for database access.
- PostgreSQL schema initialization from the backend.
- Docker Compose for a project-local PostgreSQL service.

The old `JsonFileRepository` and JSON seed file have been removed; PostgreSQL is the only active persistence architecture.

### Prioritized next steps

1. Begin backend API stabilization.
   - Add focused repository tests for schema initialization and transaction behavior.
   - Keep the complete Compose stack and `/api/health` endpoint as the baseline verification.
   - Continue documenting database startup, reset, and persistence behavior in the README.

2. Stabilize the backend API.
   - Extract flight, booking, manager, and import endpoints from `Program.cs`.
   - Add explicit request and response contracts.
   - Add centralized validation and safe error handling.
   - Protect seat changes from concurrent booking operations.

3. Complete passenger booking management.
   - Implement booking modification.
   - Enforce departed-flight and ownership rules for cancellation and modification.
   - Add cancel and modify controls with confirmation and error states in the frontend.

4. Complete manager functionality.
   - Add all requested booking filters.
   - Add the CSV upload interface and import summary.
   - Add the dynamic validation-details view.

5. Add focused tests and finish documentation.
   - Test persistence, search combinations, seat accounting, booking lifecycle, CSV parsing/import, and validation metadata.
   - Test frontend validation, loading/error states, and cancellation/modification flows.
   - Document the CSV schema, validation rules, data location, reset instructions, and known assumptions.
