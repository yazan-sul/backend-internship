# Agent Task: Scaffold Multi-Project Internship Monorepo

## Role

You are a senior full-stack engineer setting up a **monorepo scaffold** for a backend internship. This repo will host multiple independent internship projects over time, each built in its own branch, sharing one common tech stack and structure. You are building the foundation — not a specific business feature — so prioritize **genericness, isolation between projects, and zero breakage when switching branches**.

## Purpose

Each internship project (e.g. calculator app, inventory app) will live in its own git branch. All branches share the same root scaffold and tooling. Every project folder must be **fully self-contained** so that checking out any branch and running one command works immediately, with no leftover config from another project and no manual root edits required.

`main` holds only the generic scaffold, the landing page, and `ai_docs/` — never a specific project.

## Tech Stack

| Layer               | Technology                                       | Purpose                                                                                             |
| ------------------- | ------------------------------------------------ | --------------------------------------------------------------------------------------------------- |
| Frontend            | React + TypeScript + Vite                        | UI development with fast dev server & build tooling                                                 |
| Styling             | Tailwind CSS                                     | Utility-first CSS for styling components                                                            |
| Frontend Validation | Zod                                              | Schema validation for forms/API responses on the client                                             |
| Backend             | .NET (C#)                                        | Core API / business logic layer                                                                     |
| Backend Data Access | Raw SQL (no ORM)                                 | Direct SQL via ADO.NET / Npgsql — no EF Core, no Dapper for now                                     |
| Monorepo Tooling    | Bun                                              | Script runner — `bun run dev` spawns both frontend (Vite) and backend (`dotnet watch`) concurrently |
| API Communication   | REST (ASP.NET Core Web API)                      | Contract between frontend and backend                                                               |
| Database            | PostgreSQL (or SQL Server — confirm per project) | Persistent data storage                                                                             |
| Containerization    | Docker + Docker Compose                          | Environment consistency, one compose file per project                                               |

**Explicitly excluded for now:** ORM, authentication, reverse proxy. Do not add these unless asked.

## Repo Structure to Build

```
repo-root/
├── ai_docs/
│   ├── ARCHITECTURE.md        # repo/branch model, why root is generic, folder rules
│   ├── TECH_STACK.md          # the stack table above, versions, conventions
│   ├── CONVENTIONS.md         # naming, folder structure, SQL style, commit style
│   └── NEW_PROJECT.md         # step-by-step guide for starting a new project branch
├── landing/                   # React + TS + Vite + Tailwind — simple welcome page
│   ├── src/
│   ├── package.json
│   └── vite.config.ts
├── projects/
│   └── _template/             # starter skeleton, copied for every new project
│       ├── frontend/          # React + TS + Vite + Tailwind + Zod
│       ├── backend/           # .NET + C#, raw SQL data access
│       ├── docker-compose.yml # self-contained: db + backend + frontend services
│       └── package.json       # bun scripts scoped to this project only
├── package.json                # ROOT — generic, project-agnostic
├── bun.lockb
└── README.md
```

## Critical Rules (Non-Negotiable)

1. **Root must never reference a specific project by name.** No hardcoded paths like `projects/calculator` anywhere in root config.
2. **Root `bun run dev` auto-detects the current project.** On any branch, exactly one folder exists under `projects/` (aside from `_template/`, which should be excluded from detection). The root script finds that folder, `cd`s into it, and runs its local `dev` script (which itself spawns `dotnet watch` for backend and `vite` for frontend concurrently).
3. **Every project folder is fully self-contained**: its own `frontend/`, `backend/`, `docker-compose.yml`, and `package.json`. Nothing a project needs should live outside its own folder.
4. **Docker is per-project, not root-level.** Each project's `docker-compose.yml` defines its own services (db, backend, frontend if needed). No shared root compose file.
5. **Switching branches must never break the dev command.** Because each branch has only its own project folder present, this should work automatically as long as rule #1 and #2 are followed correctly — verify this explicitly by simulating a branch switch during testing.
6. **Naming convention:** project folders and git branches use lowercase-kebab-case (e.g. `inventory-app`).
7. **No ORM, no auth, no reverse proxy** in the template — keep it minimal until a project explicitly needs one.

## Build Plan (in order)

1. **Root `package.json`** with a `dev` script that runs a detection script (e.g. `scripts/run-current-project.ts`), which:
   - Scans `projects/`, ignoring `_template/`
   - Expects exactly one remaining folder — errors clearly if zero or more than one are found
   - `cd`s into that folder and runs its `dev` script
2. **`projects/_template/frontend/`** — Vite + React + TS scaffold, Tailwind configured, Zod installed, a minimal example component/schema showing the intended pattern.
3. **`projects/_template/backend/`** — minimal ASP.NET Core Web API project (.csproj, Program.cs) with one example endpoint reading from Postgres via raw SQL (Npgsql), no ORM.
4. **`projects/_template/docker-compose.yml`** — services for db + backend (+ frontend if containerized in dev), using placeholder names to be renamed per project (see NEW_PROJECT.md).
5. **`projects/_template/package.json`** — local `dev` script that runs backend (`dotnet watch run`) and frontend (`vite`) concurrently (e.g. via `concurrently` or Bun's process spawning).
6. **`landing/`** — simple React + TS + Vite + Tailwind welcome page. No routing complexity needed, just a clean landing/intro page.
7. **`ai_docs/ARCHITECTURE.md`** — explain the branch-per-project model, why root stays generic, and how auto-detection works.
8. **`ai_docs/TECH_STACK.md`** — the stack table above plus any version pins/conventions chosen during scaffolding.
9. **`ai_docs/CONVENTIONS.md`** — naming rules (kebab-case), folder conventions inside frontend/backend, raw SQL style guide, commit message style.
10. **`ai_docs/NEW_PROJECT.md`** — step-by-step instructions for starting a new project, e.g.:
    ```
    1. git checkout -b <project-name>
    2. cp -r projects/_template projects/<project-name>
    3. cd projects/<project-name>
       - rename package.json "name" field → <project-name>
       - rename backend .csproj file → <ProjectName>.csproj
       - update namespace in Program.cs → <ProjectName>
       - update docker-compose.yml service/container names → <project-name>-*
       - update DB name in connection string / compose env → <project_name>
    4. cd ../.. && bun run dev
    ```
11. **Verify:** simulate switching between two dummy project branches and confirm `bun run dev` works cleanly on each with zero manual root edits.

## Deliverable

A working file tree matching the structure above, with the root detection script functional, the `_template/` fully scaffolded and runnable, `landing/` runnable on its own, and all four `ai_docs/` files written clearly enough that any future agent or engineer joining a branch understands the rules without re-reading this prompt.

## Highlights:

- Generic root project auto-detection with lowercase-kebab-case validation.
- Self-contained React/Tailwind/Zod and ASP.NET Core/Npgsql template.
- Per-project Docker Compose stack and Dockerfiles.
- Standalone landing page.
- Complete architecture, stack, conventions, and project setup documentation.
- Root bun.lockb plus project-specific lockfiles.
- Clear zero-project and multi-project errors.

## Use a prompt like this:

Create a new internship project in:

/Users/yazansulaiman/Documents/backend-internship

Project name: inventory-app
Branch name: inventory-app
Database: PostgreSQL

Follow ai_docs/NEW_PROJECT.md and all repository conventions.

Create the project by copying projects/\_template into projects/inventory-app.
Rename all template identifiers, namespaces, package names, database names,
Docker container names, volume names, and backend project files appropriately.

Keep the project fully self-contained. Do not add an ORM, authentication,
or a reverse proxy.

Project requirements:

- [Describe the application here]
- [List the required API endpoints]
- [Describe the database tables]
- [Describe the frontend pages/forms]

After implementation:

- Run type-checks, tests, and production builds.
- Validate Docker Compose.
- Verify bun run dev starts the frontend and backend.
- Verify the health check reports the backend and PostgreSQL as healthy.

For example:

Create a new internship project in:

/Users/yazansulaiman/Documents/backend-internship

Project name: task-manager
Branch name: task-manager
Database: PostgreSQL

Follow ai_docs/NEW_PROJECT.md and all repository conventions.

Build a task-management application where users can:

- Create tasks
- List all tasks
- Mark tasks as completed
- Delete tasks

Use React, TypeScript, Tailwind, Zod, ASP.NET Core, Npgsql, and raw
parameterized SQL. Do not use an ORM or authentication.

Create the required PostgreSQL schema and REST endpoints. Keep everything
inside projects/task-manager.

Run all type-checks, tests, builds, and Docker verification when finished.

Then the new project should run from the repository root with:

bun run dev
