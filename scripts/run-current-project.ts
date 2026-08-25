import { readdir } from "node:fs/promises";
import { basename, resolve } from "node:path";

export async function listProjects(projectsDirectory: string) {
  const entries = await readdir(projectsDirectory, { withFileTypes: true });
  const projects = entries
    .filter((entry) => entry.isDirectory() && entry.name !== "_template" && !entry.name.startsWith("."))
    .map((entry) => entry.name)
    .sort();

  const invalidNames = projects.filter((name) => !/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(name));
  if (invalidNames.length > 0) {
    throw new Error(`Project folders must use lowercase-kebab-case: ${invalidNames.join(", ")}`);
  }

  return projects;
}

export async function findCurrentProject(projectsDirectory: string) {
  const projects = await listProjects(projectsDirectory);

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

export function getCurrentBranch(repositoryRoot: string) {
  const result = Bun.spawnSync(["git", "branch", "--show-current"], {
    cwd: repositoryRoot,
    stdout: "pipe",
    stderr: "ignore",
  });

  return result.exitCode === 0 ? result.stdout?.toString().trim() ?? "" : "";
}

export function selectActiveProject(projects: string[], currentBranch: string) {
  return projects.find((project) => project === currentBranch);
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

export async function runWorkspace(repositoryRoot = resolve(import.meta.dir, "..")) {
  const projectsDirectory = resolve(repositoryRoot, "projects");
  const projects = await listProjects(projectsDirectory);
  const currentBranch = getCurrentBranch(repositoryRoot);
  const activeProject = selectActiveProject(projects, currentBranch);
  const activeProjectDirectory = activeProject
    ? resolve(projectsDirectory, activeProject)
    : undefined;
  let databaseStartedByWorkspace = false;

  const services: Bun.Subprocess[] = [];

  let signalExitCode: number | undefined;
  const interrupted = new Promise<number>((resolveInterruption) => {
    process.once("SIGINT", () => {
      signalExitCode = 130;
      resolveInterruption(signalExitCode);
    });
    process.once("SIGTERM", () => {
      signalExitCode = 143;
      resolveInterruption(signalExitCode);
    });
  });

  if (activeProject) {
    console.log(`Starting ${activeProject}...`);

    const composeFile = resolve(activeProjectDirectory!, "docker-compose.yml");
    if (await Bun.file(composeFile).exists()) {
      const composeCommand = ["docker", "compose", "-f", composeFile];
      const runningDatabase = Bun.spawnSync(
        [...composeCommand, "ps", "--status", "running", "--quiet", "db"],
        { cwd: activeProjectDirectory, stdout: "pipe", stderr: "inherit" },
      );
      const databaseWasRunning =
        (runningDatabase.stdout?.toString().trim().length ?? 0) > 0;
      const databaseStart = Bun.spawnSync([...composeCommand, "up", "-d", "--wait", "db"], {
        cwd: activeProjectDirectory,
        stdin: "inherit",
        stdout: "inherit",
        stderr: "inherit",
      });
      databaseStartedByWorkspace = databaseStart.exitCode === 0 && !databaseWasRunning;
      if (databaseStart.exitCode !== 0) {
        console.error("Could not start the project database; continuing so its status is visible on the landing page.");
      }
    }

    const project = Bun.spawn(["bun", "run", "dev"], {
      cwd: activeProjectDirectory,
      stdin: "inherit",
      stdout: "inherit",
      stderr: "inherit",
      env: process.env,
    });
    services.push(project);

    console.log("Waiting for the project backend before starting the landing page...");
    for (let attempt = 0; attempt < 120 && signalExitCode === undefined; attempt += 1) {
      if (project.exitCode !== null) break;
      try {
        await fetch("http://127.0.0.1:5080/api/health", {
          signal: AbortSignal.timeout(500),
        });
        break;
      } catch {
        await Promise.race([Bun.sleep(250), interrupted]);
      }
    }

    if (project.exitCode !== null) {
      console.error(`${activeProject} exited before its backend became available.`);
      services.splice(services.indexOf(project), 1);
    }
  } else {
    const detail = currentBranch
      ? `No projects/${currentBranch} folder matches the current branch.`
      : "No Git branch is currently checked out.";
    console.log(`Starting landing page only. ${detail}`);
  }

  if (signalExitCode === undefined) {
    services.push(
      Bun.spawn(["bun", "run", "--cwd", "landing", "dev"], {
        cwd: repositoryRoot,
        stdin: "inherit",
        stdout: "inherit",
        stderr: "inherit",
        env: {
          ...process.env,
          LANDING_PORT: "5174",
          PROJECT_URL: "http://localhost:5173",
        },
      }),
    );
  }

  const exitCode = services.length > 0
    ? await Promise.race([interrupted, ...services.map((service) => service.exited)])
    : signalExitCode ?? 1;

  for (const service of services) service.kill("SIGTERM");
  await Promise.allSettled(services.map((service) => service.exited));

  if (databaseStartedByWorkspace && activeProjectDirectory) {
    console.log(`Stopping the ${activeProject} PostgreSQL service...`);
    Bun.spawnSync(["docker", "compose", "stop", "db"], {
      cwd: activeProjectDirectory,
      stdin: "inherit",
      stdout: "inherit",
      stderr: "inherit",
    });
  }

  return signalExitCode ?? exitCode;
}

if (import.meta.main) {
  try {
    process.exitCode = await runWorkspace();
  } catch (error) {
    console.error(error instanceof Error ? error.message : error);
    process.exitCode = 1;
  }
}
