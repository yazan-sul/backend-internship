# Architecture

## Branch-per-project model

`main` is the reusable starting point. It contains no business project: only repository tooling, documentation, the landing page, and `projects/_template`. A new project gets a dedicated lowercase-kebab-case branch and a copy of the template at `projects/<project-name>`.

An active project branch must contain exactly one project directory besides `_template`. Project files, dependencies, database configuration, and Docker configuration stay inside that directory. This isolation prevents branch-specific configuration from leaking into the shared scaffold or another branch.

## Generic root

Root scripts must not mention a concrete project. `bun run dev` executes `scripts/run-current-project.ts`, which scans `projects/`, ignores `_template` and hidden entries, and requires exactly one remaining directory. It then runs that directory's local `bun run dev` with the project directory as the working directory.

Zero active directories is valid on `main`, but `bun run dev` exits with guidance because there is no application to start. More than one is always an error because project selection would be ambiguous.

## Project boundary

Each `projects/<name>` directory owns:

- a React frontend and its dependencies;
- an ASP.NET Core backend and its NuGet dependencies;
- raw SQL/database access configuration;
- a Bun package defining local development commands;
- a Docker Compose stack, Dockerfiles, service names, ports, and volumes.

Projects must not import files from `_template`, `landing`, another project, or root. Shared root changes should improve every future branch and remain project-agnostic.

## Runtime flow

```text
root bun run dev
  -> detect projects/<only-active-project>
  -> project predev installs its frontend workspace
  -> project dev starts dotnet watch + Vite
  -> Vite proxies /api to ASP.NET Core
  -> ASP.NET Core accesses PostgreSQL directly through Npgsql
```

Docker Compose is an alternative full-stack runtime and belongs to each project, never to root.

The landing page can query an active local backend's `/api/health` endpoint through a development proxy. The endpoint verifies database connectivity with a lightweight `SELECT 1` and reports backend and PostgreSQL status separately.
