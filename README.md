# Backend Internship Monorepo

This repository is a generic foundation for independent internship projects. `main` contains this scaffold, the landing page, documentation, and `projects/_template`. Each project is developed on its own branch and has exactly one active folder under `projects/` in addition to `_template`.

## Start a project

Follow [`ai_docs/NEW_PROJECT.md`](ai_docs/NEW_PROJECT.md), then run:

```sh
bun run dev
```

The root command discovers the branch's only active project and delegates to its local development script. That script installs frontend packages and starts Vite plus `dotnet watch`. Start PostgreSQL first with `bun run db:up` from the project directory, or run the entire stack with `bun run docker:up`.

Run the generic landing page with:

```sh
bun run dev:landing
```

This one command starts the template PostgreSQL service, template backend, and landing Vite server. The landing page checks `http://localhost:5080/api/health` through its Vite proxy and displays separate Backend API and PostgreSQL statuses. Press Ctrl+C to stop the frontend and backend; PostgreSQL is also stopped if this command started it.

## Repository rules

- Never put a project-specific path or name in root configuration.
- Keep each project self-contained under `projects/<lowercase-kebab-case-name>`.
- Do not add an ORM, authentication, or a reverse proxy unless a project asks for it.
- Keep `main` free of active project folders.

See [`ai_docs/ARCHITECTURE.md`](ai_docs/ARCHITECTURE.md) for the full model.
