# Frontend Structure

Every project under `projects/<project-name>/frontend` should follow this structure. Keep the frontend self-contained inside its project and do not import code from the repository root, `landing`, `_template`, or another project.

## Baseline layout

```text
frontend/
├── index.html
├── package.json
├── vite.config.ts
├── tsconfig.json
├── tsconfig.app.json
├── tsconfig.node.json
├── tailwind.config.ts
├── postcss.config.js
├── Dockerfile
└── src/
    ├── main.tsx                 # Application entry point
    ├── App.tsx                  # Application shell and top-level routes/layout
    ├── index.css                # Global styles and Tailwind layers
    ├── vite-env.d.ts
    ├── components/              # Reusable UI components
    ├── features/                # Business features, added as the app grows
    ├── layouts/                 # Shared page layouts
    ├── pages/                   # Route-level page components
    ├── lib/                     # API client and small infrastructure helpers
    ├── hooks/                   # Reusable React hooks
    ├── types/                   # Shared frontend-only TypeScript types
    └── assets/                  # Images, icons, and other imported static assets
```

The template may begin with only `main.tsx`, `App.tsx`, `index.css`, and `components/`. Create additional directories only when the project needs them.

## Feature organization

Organize business code by feature instead of creating large global folders. A feature may use this shape:

```text
src/features/inventory/
├── api.ts                       # Requests and response validation
├── types.ts                     # Feature-specific types
├── components/                  # UI used only by this feature
├── hooks/                       # Hooks used only by this feature
└── pages/                       # Feature-level route pages
```

Keep a component inside its feature until it is genuinely reused. Move it to `src/components` only when it has no business ownership and is shared by multiple features.

## Responsibilities

- `main.tsx` mounts React and imports global CSS. It should contain no business logic.
- `App.tsx` owns the application shell and top-level routing. Keep page-specific logic out of it.
- `pages/` composes layouts and feature components for routes.
- `components/` contains reusable presentational UI. Prefer explicit props and accessible HTML.
- `features/` owns business workflows, feature state, feature API calls, and feature-specific UI.
- `lib/` contains shared infrastructure such as an API client. Keep it independent of individual business features.
- `hooks/` contains reusable hooks; feature-only hooks belong under that feature.
- `types/` contains types shared across multiple frontend areas. Feature-only types belong with the feature.

## API and validation

- Keep API calls in `lib/` or the owning feature’s `api.ts`, not directly in deeply nested UI components.
- Treat all network responses as untrusted and validate them with Zod before using them.
- Type request and response data explicitly. Handle loading, empty, success, and error states.
- Use relative `/api/...` URLs so Vite’s development proxy and the project’s deployment configuration remain effective.
- Do not put secrets in frontend code or committed `.env` files.

## Naming and styling

- React components and component files use PascalCase (`InventoryTable.tsx`).
- Hooks use camelCase and start with `use` (`useInventory.ts`).
- Other modules use camelCase (`apiClient.ts`).
- Keep route and feature names descriptive and consistent with the backend API.
- Use Tailwind utilities for component styling and keep global CSS limited to resets, tokens, and truly global rules.
- Avoid inline styles unless a value must be computed at runtime.

## Quality rules

- Keep components small enough to explain in one sentence; extract repeated or complex behavior.
- Prefer composition over deeply nested conditional markup.
- Include accessible labels, keyboard behavior, focus states, and useful empty/error messages.
- Do not add speculative state-management or shared abstraction layers. Introduce them only when a real feature requires them.
- Run `bun run build` from the project directory before committing frontend changes.
