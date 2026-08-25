import { readdir } from "node:fs/promises";
import { basename, resolve } from "node:path";

export async function findCurrentProject(projectsDirectory: string) {
  const entries = await readdir(projectsDirectory, { withFileTypes: true });
  const projects = entries
    .filter((entry) => entry.isDirectory() && entry.name !== "_template" && !entry.name.startsWith("."))
    .map((entry) => entry.name)
    .sort();

  const invalidNames = projects.filter((name) => !/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(name));
  if (invalidNames.length > 0) {
    throw new Error(`Project folders must use lowercase-kebab-case: ${invalidNames.join(", ")}`);
  }

  if (projects.length === 0) {
    throw new Error(
      "No active project found in projects/. Copy projects/_template to a lowercase-kebab-case project folder first.",
    );
  }

  if (projects.length > 1) {
    throw new Error(
      `Expected exactly one active project in projects/, but found ${projects.length}: ${projects.join(", ")}`,
    );
  }

  return resolve(projectsDirectory, projects[0]);
}

export async function runCurrentProject(projectsDirectory = resolve(import.meta.dir, "..", "projects")) {
  const projectDirectory = await findCurrentProject(projectsDirectory);
  console.log(`Starting ${basename(projectDirectory)}...`);

  const child = Bun.spawn(["bun", "run", "dev"], {
    cwd: projectDirectory,
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
  });

  process.once("SIGINT", () => child.kill("SIGINT"));
  process.once("SIGTERM", () => child.kill("SIGTERM"));

  return child.exited;
}

if (import.meta.main) {
  try {
    process.exitCode = await runCurrentProject();
  } catch (error) {
    console.error(error instanceof Error ? error.message : error);
    process.exitCode = 1;
  }
}
