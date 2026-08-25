# Backend Internship Monorepo

This repository is a generic foundation for independent internship projects. `main` contains this scaffold, the landing page, documentation, and `projects/_template`. Each project is developed on its own branch and has one matching folder under `projects/` in addition to `_template`.

## Start a project

Follow [`ai_docs/NEW_PROJECT.md`](ai_docs/NEW_PROJECT.md), then run:

```sh
bun run dev
```

The root command always starts the landing page at `http://localhost:5174`. When the checked-out branch has a matching `projects/<branch-name>` directory, it also starts that project's PostgreSQL service, frontend at `http://localhost:5173`, and backend at `http://localhost:5080`. The landing page lists local branches and enables **Open project** for the matching checked-out project. Press Ctrl+C to stop the processes and any database started by this command.

To run the generic landing demo with the template backend and database instead, use:

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
