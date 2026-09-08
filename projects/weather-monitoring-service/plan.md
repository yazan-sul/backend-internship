# Weather Monitoring Service — Implementation Plan

## Goal

Build a .NET 10 ASP.NET Core backend with a React frontend that accepts raw weather data, parses supported input formats, and reports activations from configured weather bots. The initial formats are JSON and XML. The initial bots are RainBot, SunBot, and SnowBot.

New formats and bot types must be easy to add without rewriting the processing workflow. The application has no database: each update is processed in memory during its HTTP request and discarded afterward.

## Scope and technology decisions

- **Backend:** .NET 10 C# ASP.NET Core Minimal API in `backend/`.
- **Frontend:** React, TypeScript, Vite, and Tailwind CSS in `frontend/`.
- **Input:** Raw JSON or XML pasted into a frontend form.
- **API:** `POST /api/weather-updates` receives a JSON request containing the raw input text.
- **Output:** The API returns normalized weather data and bot activations; React displays them.
- **Configuration:** A local JSON configuration file is loaded and validated when the backend starts.
- **Persistence:** No database or repository layer. Updates and reports are not retained.
- **Serialization:** Use `System.Text.Json` and safe `System.Xml` APIs.
- **Containers:** Docker is not required for this project.

This intentionally replaces the assignment's console prompt and printed messages with a browser form and displayed results. Parsing, configuration, thresholds, and design-pattern requirements remain unchanged.

## Intended architecture

```text
backend/
├── Program.cs                              # Composition root and endpoints
├── WeatherMonitoringService.csproj
├── appsettings.json
├── Api/
│   └── WeatherUpdateContracts.cs
├── Configuration/
│   ├── WeatherBotOptions.cs
│   └── WeatherBotOptionsValidator.cs
├── Weather/
│   └── WeatherData.cs
├── Parsing/
│   ├── IWeatherDataParser.cs
│   ├── WeatherDataParserCoordinator.cs     # Selects a registered strategy
│   ├── JsonWeatherDataParser.cs
│   └── XmlWeatherDataParser.cs
├── Monitoring/
│   ├── IWeatherObserver.cs
│   ├── WeatherMonitor.cs                   # Publishes updates to observers
│   └── BotActivation.cs
└── Bots/
    ├── RainBot.cs
    ├── SunBot.cs
    └── SnowBot.cs

frontend/src/
├── App.tsx
├── api/weatherApi.ts
├── components/
│   ├── WeatherInputForm.tsx
│   └── BotActivationList.tsx
└── schemas/weatherSchemas.ts

backend.tests/
└── ...                                     # Mirrors backend responsibilities
```

Business rules must not depend on ASP.NET Core or React. Keep responsibilities separate without introducing speculative frameworks.

## Design principles and patterns

### Strategy and Open–Closed parsing

- Define a small parser contract with `CanHandle` and `Parse` responsibilities.
- Implement JSON and XML as separate parsing strategies.
- Let `WeatherDataParserCoordinator` select from registered parsers instead of using format-specific conditionals.
- Use `CanHandle` only for recognition. Recognized but malformed input produces a format-specific error, not an "unsupported format" error.
- A new format requires a parser implementation and composition-root registration, without changes to existing parsers, monitoring, or endpoints.

### Observer bot notifications

- `WeatherMonitor` publishes successfully parsed weather updates.
- Bots are observers registered through `IWeatherObserver`.
- Each bot owns its enabled state, activation rule, name, and configured message.
- Notification produces an immutable `BotActivation` result instead of writing to a console.
- `WeatherMonitor` knows only the observer contract and collects results in registration order.
- A new bot requires its implementation, configuration, tests, and registration, but no changes to the monitor, parsers, or endpoint workflow.

### SOLID boundaries

- **Single Responsibility:** Parsing, configuration, monitoring, bot rules, HTTP transport, and UI rendering stay separate.
- **Open–Closed:** Registered strategies and observers extend behavior without modifying the pipeline.
- **Liskov Substitution:** Parser and observer implementations follow consistent success and failure contracts.
- **Interface Segregation:** Interfaces remain small and exclude HTTP or configuration details consumers do not need.
- **Dependency Inversion:** Coordinators depend on abstractions; `Program.cs` creates and registers concrete types.

Avoid generic factories, mediator libraries, repository layers, or large frameworks added only to demonstrate a pattern.

## Validation and errors

- `Location` is required and cannot be blank.
- `Temperature` must be finite. Do not impose an arbitrary temperature range absent from the assignment.
- `Humidity` must be finite and between `0` and `100`, inclusive.
- Required JSON properties and XML elements follow the documented names consistently.
- Unknown fields may be ignored; missing required values must not be invented.
- XML parsing prohibits DTD processing and external entity resolution.
- Numeric parsing behaves consistently regardless of server culture.
- Empty, unsupported, incomplete, and malformed inputs return useful `400 Bad Request` responses.
- Unexpected failures return a generic `500` response without implementation details.

## API contract

Request:

```json
{
  "rawData": "{\"Location\":\"City Name\",\"Temperature\":32,\"Humidity\":40}"
}
```

Successful response:

```json
{
  "weather": {
    "location": "City Name",
    "temperature": 32,
    "humidity": 40
  },
  "activations": [
    {
      "bot": "SunBot",
      "message": "Wow, it's a scorcher out there!"
    }
  ]
}
```

The frontend validates untrusted API responses with Zod. Frontend and backend share the documented contract, not source files.

## Implementation phases

### Phase 1 — Backend foundation

- Change the backend from a console executable to an ASP.NET Core web project.
- Add responsibility-based folders and a separate test project.
- Create immutable weather and activation models.
- Implement the validation policy and initial domain tests.
- Add `/api/health` for the repository launcher.
- Keep `Program.cs` limited to registration, middleware, and endpoint mapping.

Suggested commit: `feat: add weather monitoring backend foundation`

### Phase 2 — Bot configuration

- Add the required JSON sections for RainBot, SunBot, and SnowBot.
- Bind them to explicit strongly typed options at startup.
- Validate missing sections, non-finite thresholds, and empty messages.
- Fail startup clearly for unusable configuration rather than guessing defaults.
- Keep binding separate from activation logic.
- Test valid, disabled, missing, malformed, and invalid settings.

Suggested commit: `feat: load and validate weather bot configuration`

### Phase 3 — Extensible parsing

- Define the parser Strategy contract.
- Implement the documented JSON and XML formats.
- Add the coordinator over registered parser strategies.
- Handle empty, malformed, unsupported, and incomplete input clearly.
- Configure safe XML and culture-independent number parsing.
- Test successful and unsuccessful JSON/XML inputs.

Suggested commit: `feat: parse JSON and XML weather updates`

### Phase 4 — Observer pipeline and bots

- Define the observer contract and immutable activation result.
- Implement monitor notification over registered observers.
- RainBot activates when humidity is strictly greater than its threshold.
- SunBot activates when temperature is strictly greater than its threshold.
- SnowBot activates when temperature is strictly less than its threshold.
- Disabled bots and exact threshold equality never activate.
- Test activation, non-activation, equality, disabled bots, multiple results, and result order.

Suggested commit: `feat: notify configured weather bots`

### Phase 5 — HTTP workflow

- Add request and response DTOs that do not expose implementation types.
- Add `POST /api/weather-updates` to parse and publish one update.
- Return normalized weather plus every activation.
- Map expected input failures to consistent `400` responses.
- Keep endpoints thin and free of format-specific or bot-specific conditions.
- Add integration tests for JSON, XML, errors, no activations, and multiple activations.

Suggested commit: `feat: expose weather update endpoint`

### Phase 6 — Frontend interaction

- Replace the placeholder with an accessible raw-data textarea and submit action.
- Provide JSON and XML examples without duplicating parsing or bot rules in TypeScript.
- Render normalized weather values and activated bot messages.
- Show loading, validation, server-error, no-activation, and success states.
- Prevent duplicate submissions while a request is pending.
- Configure the Vite `/api` development proxy.
- Keep the layout responsive and keyboard accessible.
- Add focused frontend tests where practical.

Suggested commit: `feat: add weather  onitoring interface`

### Phase 7 — Verification and documentation

- Test malformed and unsupported input, missing fields, non-finite numbers, humidity boundaries, and unsafe XML.
- Test exact thresholds, disabled bots, multiple activations, and no activations.
- Test invalid, missing, and partial configuration.
- Confirm a sample parser and observer can be added without changing the workflow.
- Confirm concurrent requests do not share activation results or retain weather data.
- Add project README instructions for configuration, development, tests, and builds.
- Run frontend typecheck/build, .NET build, and all tests.

Suggested commit: `feat: complete weather monitoring service`

## Configuration shape

Keep the assignment's JSON structure and casing:

```json
{
  "RainBot": {
    "enabled": true,
    "humidityThreshold": 70,
    "message": "It looks like it's about to pour down!"
  },
  "SunBot": {
    "enabled": true,
    "temperatureThreshold": 30,
    "message": "Wow, it's a scorcher out there!"
  },
  "SnowBot": {
    "enabled": false,
    "temperatureThreshold": 0,
    "message": "Brrr, it's getting chilly!"
  }
}
```

Use explicit JSON names or intentional serializer settings so casing is not accidental. Strongly typed settings for the three required bots are preferable to a complex generic configuration framework.

## Testing approach

- **Domain:** Required location, finite values, and humidity boundaries.
- **Parsers:** Valid/invalid JSON and XML, unsupported input, culture behavior, and XML security.
- **Configuration:** Valid, disabled, missing, malformed, and invalid settings.
- **Bots:** Rules, equality boundaries, messages, and enabled state.
- **Observer:** Multiple observers, deterministic results, and concrete-type isolation.
- **API:** Contract, status codes, parser selection, activations, and error mapping.
- **Frontend:** Submission states, response validation, result rendering, and accessible errors.

Prefer public-behavior tests over tests coupled to private implementation details.

## Decisions and non-goals

- **No console:** Browser input and API responses replace standard input and printing.
- **No database:** "In memory" means transient request processing, not a global history collection.
- **Real-time:** The user submits an update and immediately receives a result. Physical station networking, queues, polling, SignalR, and WebSockets are outside scope.
- **Bot lifetime:** Reused bot instances must remain immutable and stateless after configuration.
- **Configuration failure:** Invalid required bot settings prevent backend startup.
- **Activation order:** Results follow registration order: RainBot, SunBot, SnowBot.
- **Future history:** If requested later, add an explicit storage abstraction then; do not add an in-memory repository preemptively.

## Completion criteria

- Backend configuration controls all bot enabled states, thresholds, and messages.
- The frontend submits the documented JSON or XML weather shape.
- Valid input produces one normalized weather model.
- Enabled bots activate only under their strict configured conditions.
- The API returns activation names/messages and the frontend displays them.
- Invalid input produces useful errors without crashing the backend or UI.
- Parsers and bots can be extended without changing the monitoring or endpoint workflow.
- No database or persistent weather history is introduced.
- Business rules remain independent of HTTP and UI code.
- Backend tests/build and frontend typecheck/build pass.

## Git workflow

Use focused imperative Conventional Commit messages and inspect changed files before committing. Do not commit secrets, generated build output, dependency directories, or unrelated changes. Pushing to a remote is a separate explicit action, not an application completion requirement.
