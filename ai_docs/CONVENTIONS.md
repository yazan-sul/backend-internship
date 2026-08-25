# Conventions

## Naming and layout

- Project branches and directories: lowercase-kebab-case (`inventory-app`).
- C# projects, namespaces, and public types: PascalCase (`InventoryApp`).
- TypeScript files containing components: PascalCase; other modules: camelCase.
- API routes: lowercase kebab-case nouns under `/api`.
- PostgreSQL identifiers: lowercase snake_case.
- Keep frontend code under `frontend/src` and backend code under `backend`.
- Organize by feature as a project grows; do not build speculative shared layers.

## SQL

- Use explicit column lists; avoid `SELECT *`.
- Always parameterize values. Never concatenate user input into SQL.
- Use `await using` for connections, commands, readers, and transactions where supported.
- Keep statements readable with uppercase SQL keywords and lowercase snake_case identifiers.
- Wrap related writes in a transaction and state the intended isolation level when it matters.
- Store migrations or setup scripts in the owning project's `backend/Database` directory when introduced.

## API and validation

- Return JSON with consistent HTTP status codes.
- Pass `CancellationToken` through database operations.
- Treat network data as untrusted; validate frontend inputs and API responses with Zod.
- Keep secrets in environment variables or ignored `.env` files, never committed source.

## Git

Use focused commits with imperative Conventional Commit messages, for example:

```text
feat: add inventory item endpoint
fix: parameterize product lookup query
docs: explain local database setup
```

Never mix a generic scaffold change with project business functionality.

