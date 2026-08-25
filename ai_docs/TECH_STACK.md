# Tech Stack

| Layer | Technology | Convention |
| --- | --- | --- |
| Frontend | React 19, TypeScript, Vite | Functional components, strict TypeScript |
| Styling | Tailwind CSS 3.4 | Utility classes; small global base stylesheet |
| Client validation | Zod 4 | Validate form values and untrusted API responses |
| Backend | .NET 10 / C# / ASP.NET Core Minimal API | Nullable reference types enabled |
| Data access | Npgsql 10 and ADO.NET | Parameterized raw SQL; no ORM or Dapper |
| Monorepo runner | Bun 1.3+ | Root detection plus project-local process orchestration |
| API | REST over HTTP | JSON payloads under `/api` |
| Database | PostgreSQL 18 by default | Confirm PostgreSQL versus SQL Server per project |
| Containers | Docker and Docker Compose | One Compose file per project |

Package versions are recorded by lockfiles after install. Root retains the requested binary `bun.lockb`; project packages use Bun's current text `bun.lock` format. The template intentionally uses current frontend packages while pinning Tailwind 3.4 because its configuration is stable and explicit. The .NET target and Docker images are aligned at .NET 10.

Do not add an ORM, authentication, or a reverse proxy to the scaffold. These are project decisions.

For root development, the landing Vite server listens on `5174`, the active project's Vite server on `5173`, and ASP.NET Core on `5080`. The PostgreSQL container is exposed on host port `55432` to avoid collisions with an existing local PostgreSQL installation. PostgreSQL continues to listen on `5432` inside the Compose network. A project may change host ports within its own directory if necessary.
