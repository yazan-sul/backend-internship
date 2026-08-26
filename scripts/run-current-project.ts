import { readdir } from "node:fs/promises";
import { basename, resolve } from "node:path";

const CONTROL_HOST = "127.0.0.1";
const CONTROL_PORT = Number(process.env.WORKSPACE_CONTROL_PORT ?? 5090);
const LANDING_PORT = Number(process.env.LANDING_PORT ?? 5174);
const LANDING_URL = `http://localhost:${LANDING_PORT}`;
const PROJECT_URL = "http://localhost:5173";
const ALLOWED_ORIGINS = new Set([LANDING_URL, "http://127.0.0.1:5174"]);

type BranchInfo = { name: string; hasProject: boolean };
type RunningWorkspace = {
  services: Bun.Subprocess[];
  databaseStarted: boolean;
  projectDirectory?: string;
};
type WorkspaceRuntime = {
  switchingTo?: string;
  lastError?: string;
  projectReady: boolean;
  landingReady: boolean;
};

export async function listProjects(projectsDirectory: string) {
  const entries = await readdir(projectsDirectory, { withFileTypes: true });
  const projects = entries
    .filter(
      (entry) =>
        entry.isDirectory() &&
        entry.name !== "_template" &&
        !entry.name.startsWith("."),
    )
    .map((entry) => entry.name)
    .sort();

  const invalidNames = projects.filter(
    (name) => !/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(name),
  );
  if (invalidNames.length > 0) {
    throw new Error(
      `Project folders must use lowercase-kebab-case: ${invalidNames.join(", ")}`,
    );
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

function git(
  repositoryRoot: string,
  arguments_: string[],
  output: "pipe" | "inherit" = "pipe",
) {
  return Bun.spawnSync(["git", ...arguments_], {
    cwd: repositoryRoot,
    stdin: "inherit",
    stdout: output,
    stderr: output,
  });
}

function gitText(repositoryRoot: string, arguments_: string[]) {
  const result = git(repositoryRoot, arguments_);
  return result.exitCode === 0 ? (result.stdout?.toString().trim() ?? "") : "";
}

export function getCurrentBranch(repositoryRoot: string) {
  return gitText(repositoryRoot, ["branch", "--show-current"]);
}

export function selectActiveProject(projects: string[], currentBranch: string) {
  return projects.find((project) => project === currentBranch);
}

function getLocalBranches(repositoryRoot: string): BranchInfo[] {
  const names = gitText(repositoryRoot, ["branch", "--format=%(refname:short)"])
    .split("\n")
    .filter(Boolean);
  return names.map((name) => ({
    name,
    hasProject:
      git(repositoryRoot, [
        "cat-file",
        "-e",
        `${name}:projects/${name}/package.json`,
      ]).exitCode === 0,
  }));
}

function hasUncommittedChanges(repositoryRoot: string) {
  return gitText(repositoryRoot, ["status", "--porcelain"]).length > 0;
}

async function getWorkspaceState(
  repositoryRoot: string,
  runtime: WorkspaceRuntime,
) {
  const currentBranch = getCurrentBranch(repositoryRoot);
  const projects = await listProjects(resolve(repositoryRoot, "projects"));
  return {
    branches: getLocalBranches(repositoryRoot),
    currentBranch,
    activeProject: selectActiveProject(projects, currentBranch) ?? null,
    dirty: hasUncommittedChanges(repositoryRoot),
    switchingTo: runtime.switchingTo ?? null,
    projectReady: runtime.projectReady,
    landingReady: runtime.landingReady,
    projectUrl: PROJECT_URL,
    lastError: runtime.lastError ?? null,
  };
}

function corsHeaders(origin: string | null) {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    "Cache-Control": "no-store",
    Vary: "Origin",
  };
  if (origin && ALLOWED_ORIGINS.has(origin))
    headers["Access-Control-Allow-Origin"] = origin;
  return headers;
}

function json(data: unknown, status: number, origin: string | null) {
  return Response.json(data, { status, headers: corsHeaders(origin) });
}

function createSwitchQueue() {
  const queued: string[] = [];
  let wake: ((branch: string) => void) | undefined;
  return {
    push(branch: string) {
      if (wake) {
        const resolveSwitch = wake;
        wake = undefined;
        resolveSwitch(branch);
      } else {
        queued.push(branch);
      }
    },
    next() {
      const branch = queued.shift();
      return branch
        ? Promise.resolve(branch)
        : new Promise<string>((resolveSwitch) => (wake = resolveSwitch));
    },
  };
}

async function isReachable(url: string) {
  try {
    await fetch(url, { signal: AbortSignal.timeout(500) });
    return true;
  } catch {
    return false;
  }
}

async function waitUntilReachable(urls: string[], process?: Bun.Subprocess) {
  for (let attempt = 0; attempt < 120; attempt += 1) {
    if (process?.exitCode !== null) return false;
    if ((await Promise.all(urls.map(isReachable))).every(Boolean)) return true;
    await Bun.sleep(250);
  }
  return false;
}

function killProcessGroup(service: Bun.Subprocess, signal: NodeJS.Signals) {
  try {
    process.kill(-service.pid, signal);
  } catch {
    service.kill(signal);
  }
}

function composeHasService(composeFile: string, service: string, cwd: string) {
  const result = Bun.spawnSync(
    ["docker", "compose", "-f", composeFile, "config", "--services"],
    {
      cwd,
      stdout: "pipe",
      stderr: "pipe",
    },
  );
  return (
    result.exitCode === 0 &&
    result.stdout
      ?.toString()
      .split("\n")
      .some((name) => name.trim() === service)
  );
}

async function startWorkspaceServices(
  repositoryRoot: string,
  runtime: WorkspaceRuntime,
  openBrowser: boolean,
): Promise<RunningWorkspace> {
  runtime.projectReady = false;
  runtime.landingReady = false;

  const projectsDirectory = resolve(repositoryRoot, "projects");
  const currentBranch = getCurrentBranch(repositoryRoot);
  const projects = await listProjects(projectsDirectory);
  const activeProject = selectActiveProject(projects, currentBranch);
  const services: Bun.Subprocess[] = [];
  let databaseStarted = false;
  let projectDirectory: string | undefined;

  if (activeProject) {
    projectDirectory = resolve(projectsDirectory, activeProject);
    console.log(`Starting ${activeProject}...`);

    const composeFile = resolve(projectDirectory, "docker-compose.yml");
    if (
      (await Bun.file(composeFile).exists()) &&
      composeHasService(composeFile, "db", projectDirectory)
    ) {
      const compose = ["docker", "compose", "-f", composeFile];
      const running = Bun.spawnSync(
        [...compose, "ps", "--status", "running", "--quiet", "db"],
        {
          cwd: projectDirectory,
          stdout: "pipe",
          stderr: "inherit",
        },
      );
      const wasRunning = (running.stdout?.toString().trim().length ?? 0) > 0;
      const started = Bun.spawnSync([...compose, "up", "-d", "--wait", "db"], {
        cwd: projectDirectory,
        stdin: "inherit",
        stdout: "inherit",
        stderr: "inherit",
      });
      databaseStarted = started.exitCode === 0 && !wasRunning;
      if (started.exitCode !== 0)
        runtime.lastError = "Could not start the project database.";
    }

    const project = Bun.spawn(["bun", "run", "--no-orphans", "dev"], {
      cwd: projectDirectory,
      stdin: "inherit",
      stdout: "inherit",
      stderr: "inherit",
      env: process.env,
      detached: true,
    });
    services.push(project);
    runtime.projectReady = await waitUntilReachable(
      ["http://127.0.0.1:5080/api/health", PROJECT_URL],
      project,
    );
    if (!runtime.projectReady)
      runtime.lastError = `${activeProject} did not become ready within 30 seconds.`;
  } else {
    const detail = currentBranch
      ? `No projects/${currentBranch} folder matches the current branch.`
      : "No Git branch is currently checked out.";
    console.log(`Starting landing page only. ${detail}`);
  }

  const landingInstall = Bun.spawnSync(["bun", "install", "--cwd", "landing"], {
    cwd: repositoryRoot,
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
  });
  if (landingInstall.exitCode !== 0) {
    throw new Error("Could not install landing page dependencies.");
  }

  const landing = Bun.spawn(
    ["bun", "run", "--no-orphans", "--cwd", "landing", "dev"],
    {
      cwd: repositoryRoot,
      stdin: "inherit",
      stdout: "inherit",
      stderr: "inherit",
      env: {
        ...process.env,
        LANDING_PORT: String(LANDING_PORT),
        OPEN_BROWSER: openBrowser ? "true" : "false",
        VITE_WORKSPACE_CONTROL_URL: `http://${CONTROL_HOST}:${CONTROL_PORT}`,
      },
      detached: true,
    },
  );
  services.push(landing);
  runtime.landingReady = await waitUntilReachable([LANDING_URL], landing);
  return { services, databaseStarted, projectDirectory };
}

async function stopWorkspaceServices(workspace: RunningWorkspace) {
  for (const service of workspace.services)
    killProcessGroup(service, "SIGTERM");
  const gracefulShutdown = Promise.allSettled(
    workspace.services.map((service) => service.exited),
  );
  const exitedGracefully = await Promise.race([
    gracefulShutdown.then(() => true),
    Bun.sleep(5000).then(() => false),
  ]);

  if (!exitedGracefully) {
    console.warn("Services did not stop within 5 seconds; forcing shutdown...");
    for (const service of workspace.services) {
      if (service.exitCode === null) killProcessGroup(service, "SIGKILL");
    }
    await Promise.race([gracefulShutdown, Bun.sleep(2000)]);
  }

  if (workspace.databaseStarted && workspace.projectDirectory) {
    console.log("Stopping the project PostgreSQL service...");
    Bun.spawnSync(["docker", "compose", "stop", "--timeout", "2", "db"], {
      cwd: workspace.projectDirectory,
      stdin: "inherit",
      stdout: "inherit",
      stderr: "inherit",
    });
  }
}

export async function runCurrentProject(
  projectsDirectory = resolve(import.meta.dir, "..", "projects"),
) {
  const projectDirectory = await findCurrentProject(projectsDirectory);
  console.log(`Starting ${basename(projectDirectory)}...`);
  const child = Bun.spawn(["bun", "run", "--no-orphans", "dev"], {
    cwd: projectDirectory,
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
    detached: true,
  });
  process.once("SIGINT", () => killProcessGroup(child, "SIGINT"));
  process.once("SIGTERM", () => killProcessGroup(child, "SIGTERM"));
  return child.exited;
}

export async function runWorkspace(
  repositoryRoot = resolve(import.meta.dir, ".."),
) {
  const runtime: WorkspaceRuntime = {
    projectReady: false,
    landingReady: false,
  };
  const switchQueue = createSwitchQueue();

  const controlServer = Bun.serve({
    hostname: CONTROL_HOST,
    port: CONTROL_PORT,
    async fetch(request) {
      const url = new URL(request.url);
      const origin = request.headers.get("Origin");
      if (origin && !ALLOWED_ORIGINS.has(origin)) {
        return json({ error: "Origin not allowed." }, 403, null);
      }
      if (request.method === "OPTIONS") {
        return new Response(null, {
          status: 204,
          headers: {
            ...corsHeaders(origin),
            "Access-Control-Allow-Headers": "Content-Type",
            "Access-Control-Allow-Methods": "GET, POST, OPTIONS",
          },
        });
      }
      if (url.pathname === "/workspace/state" && request.method === "GET") {
        return json(
          await getWorkspaceState(repositoryRoot, runtime),
          200,
          origin,
        );
      }
      if (url.pathname === "/workspace/switch" && request.method === "POST") {
        if (!origin || !ALLOWED_ORIGINS.has(origin)) {
          return json(
            {
              error:
                "Branch switching is only allowed from the local landing page.",
            },
            403,
            origin,
          );
        }
        if (runtime.switchingTo)
          return json(
            { error: "A branch switch is already running." },
            409,
            origin,
          );

        const body = (await request.json().catch(() => null)) as {
          branch?: unknown;
        } | null;
        const branch = typeof body?.branch === "string" ? body.branch : "";
        const branches = getLocalBranches(repositoryRoot);
        if (!branches.some((candidate) => candidate.name === branch)) {
          return json({ error: "Unknown local branch." }, 400, origin);
        }
        if (hasUncommittedChanges(repositoryRoot)) {
          return json(
            { error: "Commit or stash all changes before switching branches." },
            409,
            origin,
          );
        }
        if (branch === getCurrentBranch(repositoryRoot)) {
          return json({ ok: true, currentBranch: branch }, 200, origin);
        }

        runtime.switchingTo = branch;
        runtime.lastError = undefined;
        switchQueue.push(branch);
        return json({ ok: true, switchingTo: branch }, 202, origin);
      }
      return json({ error: "Not found." }, 404, origin);
    },
  });

  console.log(
    `Local workspace control: http://${CONTROL_HOST}:${CONTROL_PORT}`,
  );

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

  let openBrowser = true;
  let exitCode = 0;
  while (signalExitCode === undefined) {
    const workspace = await startWorkspaceServices(
      repositoryRoot,
      runtime,
      openBrowser,
    );
    openBrowser = false;
    runtime.switchingTo = undefined;

    const event = await Promise.race([
      interrupted.then((code) => ({ type: "signal" as const, code })),
      switchQueue
        .next()
        .then((branch) => ({ type: "switch" as const, branch })),
      ...workspace.services.map((service) =>
        service.exited.then((code) => ({ type: "exit" as const, code })),
      ),
    ]);

    await stopWorkspaceServices(workspace);
    runtime.projectReady = false;
    runtime.landingReady = false;

    if (event.type === "signal") {
      exitCode = event.code;
      break;
    }
    if (event.type === "exit") {
      exitCode = event.code;
      break;
    }

    console.log(`Switching to ${event.branch}...`);
    const switched = git(repositoryRoot, ["switch", event.branch], "inherit");
    if (switched.exitCode !== 0)
      runtime.lastError = `Git could not switch to ${event.branch}.`;
  }

  await controlServer.stop(true);
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
