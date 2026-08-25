# Backend Internship Monorepo

This repository is a generic foundation for independent internship projects. `main` contains this scaffold, the landing page, documentation, and `projects/_template`. Each project is developed on its own branch and has one matching folder under `projects/` in addition to `_template`.

## Start a project

Follow [`ai_docs/NEW_PROJECT.md`](ai_docs/NEW_PROJECT.md), then run:

```sh
bun run dev
```

The root command always starts the landing page at `http://localhost:5174`. When the checked-out branch has a matching `projects/<branch-name>` directory, it also starts that project's PostgreSQL service, frontend at `http://localhost:5173`, and backend at `http://localhost:5080`. Press Ctrl+C to stop the processes and any database started by this command.

The landing page includes a local-only branch switcher. **Switch & open** asks the workspace supervisor on `127.0.0.1:5090` to stop current services, run a safe `git switch`, start the target project, and redirect when it is ready. Switching is disabled whenever the worktree contains staged, unstaged, or untracked changes. Commit or stash first; the launcher never forces a switch or discards work.

`dev:landing` is retained as an alias for the same workspace launcher:

```sh
bun run dev:landing
```

The branch switcher is a development tool and is not available in production builds. Every project branch must contain the shared launcher and landing changes for the dashboard to remain available after switching.

## Repository rules

- Never put a project-specific path or name in root configuration.
- Keep each project self-contained under `projects/<lowercase-kebab-case-name>`.
- Do not add an ORM, authentication, or a reverse proxy unless a project asks for it.
- Keep `main` free of active project folders.

See [`ai_docs/ARCHITECTURE.md`](ai_docs/ARCHITECTURE.md) for the full model.
