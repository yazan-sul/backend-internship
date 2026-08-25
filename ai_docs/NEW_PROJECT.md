# Start a New Project

Replace `<project-name>` with a lowercase-kebab-case name such as `inventory-app`, and `<ProjectName>` with its PascalCase form such as `InventoryApp`.

1. Start from the latest generic scaffold and create the project branch.

   ```sh
   git checkout main
   git checkout -b <project-name>
   cp -R projects/_template projects/<project-name>
   ```

2. In `projects/<project-name>/package.json`, change `name` to `<project-name>`.

3. Rename the backend project file and update every reference to it.

   ```sh
   mv projects/<project-name>/backend/ProjectTemplate.csproj projects/<project-name>/backend/<ProjectName>.csproj
   ```

   Replace `ProjectTemplate` with `<ProjectName>` in:

   - `backend/<ProjectName>.csproj` (`RootNamespace`);
   - `backend/Dockerfile` (project file and DLL);
   - `scripts/dev.ts` (project path);
   - any namespaces added to backend C# files.

4. In `docker-compose.yml`, replace `project-template` with `<project-name>` for container and volume names. Replace `project_template` with a lowercase snake_case database name such as `inventory_app` in Compose and `backend/appsettings.json`.

   Keep the container database port at `5432`. The template uses host port `55432`; choose another unused host port per project when multiple project databases must run simultaneously, and update `backend/appsettings.json` to match.

5. Rename frontend package metadata, page titles, and visible template copy. Keep configuration inside this project folder.

6. Confirm the branch has only `_template` and the new project under `projects/`.

   ```sh
   ls projects
   ```

7. Start PostgreSQL, then start the landing page, frontend, and backend together from root.

   ```sh
   cd projects/<project-name>
   bun run db:up
   cd ../..
   bun run dev
   ```

   Alternatively, run `bun run docker:up` inside the project directory for the entire containerized stack.

8. Open the landing page at `http://localhost:5174`, confirm the current branch is highlighted, and use **Open project** to reach the project at `http://localhost:5173`. Verify the health check, then run `bun test` at root and `bun run build` inside the project before committing.

Do not edit the root detector to add the project name. The directory is discovered automatically, which is what keeps branch switching safe.
