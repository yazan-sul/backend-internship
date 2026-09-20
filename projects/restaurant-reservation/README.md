# Restaurant Reservation Management System

A self-contained restaurant operations project built with React, Vite, Tailwind CSS, ASP.NET Core Minimal API, raw Npgsql, PostgreSQL, Bun, and Docker.

## Current status

- Phase 1: project scaffold and naming complete.
- Phase 2: relational design and entity relationship diagram complete.
- Database schema, seed data, API features, and management interface are planned next.

## Design documentation

- [Relational design](docs/relational-design.md)
- [Entity relationship diagram source](docs/erd/restaurant-reservation-erd.mmd)
- [Rendered entity relationship diagram](docs/erd/restaurant-reservation-erd.svg)
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

