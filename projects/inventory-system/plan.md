# Simple Inventory Management System — End-to-End Implementation Plan

## Goal

Build a full-stack inventory management web application. Users will manage products through a browser interface, while the ASP.NET backend exposes the API and PostgreSQL stores the data permanently.

## Technology and architecture

- **Frontend:** React, TypeScript, and Tailwind CSS in `frontend/`.
- **Backend:** ASP.NET Core Web API in `backend/`.
- **Database:** PostgreSQL running through Docker Compose.
- **Data flow:** React UI → HTTP API → PostgreSQL database.
- **Core model:** A product has a name, price, and quantity in stock.

## Phases and Git checkpoints

### Phase 1 — Project setup and database foundation

- Confirm the existing frontend, backend, and Docker development setup.
- Create the product database table with an ID, name, price, quantity, and timestamps.
- Add database connection configuration using environment variables or local development settings.
- Add the initial database setup or migration under the backend project.
- Verify the database starts and the backend can connect to it.

Commit and push:

```text
feat: set up product database foundation
```

### Phase 2 — Product model and backend read API

- Create the backend product model and database mapping.
- Add request and response DTOs so API contracts are explicit.
- Implement `GET /api/products` to return all products.
- Implement `GET /api/products/{id}` for retrieving one product.
- Return consistent HTTP status codes and JSON responses.
- Add backend validation for names, prices, and quantities.

Commit and push:

```text
feat: add product model and read endpoints
```

### Phase 3 — Add product end to end

- Implement `POST /api/products` in the backend.
- Validate required fields, non-negative price, and non-negative quantity.
- Add the frontend API client and product types.
- Build an inventory page with an Add Product form.
- Display success and error feedback without reloading the page.
- Refresh the product list after a successful creation.

Commit and push:

```text
feat: add products through the inventory UI
```

### Phase 4 — View inventory

- Build the product table or card list in the frontend.
- Display product name, price, quantity, and available actions.
- Add loading, empty-state, and API-error states.
- Format currency and quantities consistently.
- Verify the UI loads persisted products from the backend.

Commit and push:

```text
feat: display persisted inventory products
```

### Phase 5 — Edit product end to end

- Implement `PUT /api/products/{id}` in the backend.
- Add an edit form or modal in the frontend.
- Allow updating the name, price, and quantity.
- Reuse shared validation on both frontend and backend.
- Update the product list after a successful edit.
- Handle missing products and validation errors clearly.

Commit and push:

```text
feat: edit products from the inventory UI
```

### Phase 6 — Delete product end to end

- Implement `DELETE /api/products/{id}` in the backend.
- Add a delete action to each product row or card.
- Ask for confirmation before deletion.
- Remove the product from the UI after a successful response.
- Handle missing products and failed requests without corrupting displayed state.

Commit and push:

```text
feat: delete products from the inventory UI
```

### Phase 7 — Search for a product

- Add a search input to the inventory page.
- Implement server-side filtering with `GET /api/products?search=...` or document a client-side search strategy.
- Display matching products and a helpful no-results state.
- Debounce requests if search is server-side.
- Preserve loading and error behavior during searches.

Commit and push:

```text
feat: add product search
```

### Phase 8 — Integration testing and final polish

- Add backend tests for listing, creation, validation, editing, deletion, searching, and not-found responses.
- Add frontend tests for loading, form submission, edit/delete actions, search, empty state, and API errors where supported.
- Test the complete flow against PostgreSQL through Docker.
- Improve responsive layout, accessibility labels, keyboard navigation, and user feedback.
- Add README instructions for environment setup, database startup, development, and production builds.
- Verify frontend build, backend build, type checks, and tests.
- Confirm every phase commit has been pushed to the intended GitHub repository.

Final commit and push:

```text
feat: complete end-to-end inventory management system
```

## GitHub workflow

At the end of every phase:

```sh
git status
git add <changed-files>
git commit -m "<phase commit message>"
git push origin <branch-name>
```

Use focused, imperative Conventional Commit messages. Push each completed phase before starting the next one. Do not commit secrets, generated files, or unrelated changes.

## Completion criteria

- A user can open the web application and manage products without using a console.
- Products are persisted in PostgreSQL and remain available after restarting the application.
- Add, view, edit, delete, and search work through the browser and API.
- Frontend and backend both validate user input.
- Loading, empty, validation, not-found, and server-error states are handled clearly.
- The application is responsive and accessible for normal keyboard and screen-reader use.
- The frontend and backend build successfully, tests pass, and all phase commits are pushed to GitHub.
