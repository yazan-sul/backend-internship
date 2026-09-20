# Restaurant Reservation Management System

A self-contained restaurant operations project built with React, Vite, Tailwind CSS, ASP.NET Core Minimal API, raw Npgsql, PostgreSQL, Bun, and Docker.

## Current status

- Phase 1: project scaffold and naming complete.
- Phase 2: relational design and entity relationship diagram complete.
- Phase 3: PostgreSQL schema and integrity validation complete.
- Seed data, API features, and management interface are planned next.

## Design documentation

- [Entity relationship diagram](docs/erd/restaurant-erd.png)
- [PostgreSQL schema](database/01-schema.sql)
- [Schema validation](database/tests/01-schema-validation.sql)
- [Implementation plan](PLAN.md)

## Architecture

```text
React/Vite/Tailwind frontend
           |
           | /api/*
           v
ASP.NET Core Minimal API
           |
           | parameterized Npgsql commands
           v
PostgreSQL 18
```

All project code and configuration live in this directory. The backend uses raw SQL rather than an ORM.
