# Architecture

## Branch-per-project model

`main` is the reusable starting point. It contains no committed business project: only repository tooling, documentation, the landing page, and `projects/_template`. A new project gets a dedicated lowercase-kebab-case branch and a copy of the template at `projects/<project-name>`.

An active project branch must contain a project directory whose name exactly matches the branch. Project files, dependencies, database configuration, and Docker configuration stay inside that directory. This isolation prevents branch-specific configuration from leaking into the shared scaffold or another branch.

## Generic root

Root scripts must not mention a concrete project. `bun run dev` executes `scripts/run-current-project.ts`, reads the current Git branch, and scans `projects/` while ignoring `_template` and hidden entries. The landing page always starts on port `5174`. If `projects/<current-branch>` exists, its PostgreSQL Compose service and local development command also start, with its frontend on its conventional port `5173` and backend using port `5080`.

Zero matching directories is valid on `main`; `bun run dev` still serves the landing page. A project directory left over from another branch is never activated because its name does not match the checked-out branch.

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
  -> read local branches and the checked-out branch
  -> always start landing on :5174
  -> when projects/<current-branch> exists, run its local dev command
  -> start that project's PostgreSQL Compose service
  -> project frontend starts on :5173 and proxies /api to ASP.NET Core on :5080
  -> ASP.NET Core accesses PostgreSQL directly through Npgsql
```

Docker Compose is an alternative full-stack runtime and belongs to each project, never to root.

The landing page can query an active local backend's `/api/health` endpoint through a development proxy. The endpoint verifies database connectivity with a lightweight `SELECT 1` and reports backend and PostgreSQL status separately.
