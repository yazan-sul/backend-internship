import { resolve } from "node:path";

const repositoryRoot = resolve(import.meta.dir, "..");
const composeFile = resolve(repositoryRoot, "projects", "_template", "docker-compose.yml");
const backendProject = resolve(
  repositoryRoot,
  "projects",
  "_template",
  "backend",
  "ProjectTemplate.csproj",
);
const composeCommand = ["docker", "compose", "-f", composeFile];

function runCompose(arguments_: string[], stdout: "pipe" | "inherit" = "inherit") {
  return Bun.spawnSync([...composeCommand, ...arguments_], {
    cwd: repositoryRoot,
    stdin: "inherit",
    stdout,
    stderr: "inherit",
  });
}

const runningDatabase = runCompose(["ps", "--status", "running", "--quiet", "db"], "pipe");
if (runningDatabase.exitCode !== 0) {
  console.error("Could not inspect PostgreSQL. Is Docker running?");
  process.exit(1);
}

const databaseWasAlreadyRunning = (runningDatabase.stdout?.toString().trim().length ?? 0) > 0;
const databaseStart = runCompose(["up", "-d", "--wait", "db"]);
if (databaseStart.exitCode !== 0) {
  console.error("Could not start PostgreSQL.");
  process.exit(databaseStart.exitCode);
}

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

const backend = Bun.spawn(["dotnet", "watch", "--no-hot-reload", "run", "--project", backendProject], {
  cwd: repositoryRoot,
  stdin: "inherit",
  stdout: "inherit",
  stderr: "inherit",
  env: { ...process.env, ASPNETCORE_ENVIRONMENT: "Development" },
});
const services = [backend];

console.log("Waiting for the backend and PostgreSQL health check...");
let backendIsHealthy = false;
for (let attempt = 0; attempt < 120 && signalExitCode === undefined; attempt += 1) {
  if (backend.exitCode !== null) break;

  try {
    const response = await fetch("http://127.0.0.1:5080/api/health", {
      signal: AbortSignal.timeout(500),
    });
    if (response.ok) {
      backendIsHealthy = true;
      break;
    }
  } catch {
    // The backend is still restoring, building, or starting.
  }

  await Promise.race([Bun.sleep(250), interrupted]);
}

let exitCode: number;
if (!backendIsHealthy) {
  if (signalExitCode === undefined) {
    console.error("The backend did not become healthy within 30 seconds.");
  }
  exitCode = signalExitCode ?? 1;
} else {
  console.log("Backend and PostgreSQL are healthy. Starting the landing page...");
  const frontend = Bun.spawn(["bun", "run", "--cwd", "landing", "dev"], {
    cwd: repositoryRoot,
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
  });
  services.push(frontend);
  exitCode = await Promise.race([
    interrupted,
    ...services.map((service) => service.exited),
  ]);
}

for (const service of services) service.kill("SIGTERM");
await Promise.allSettled(services.map((service) => service.exited));

if (!databaseWasAlreadyRunning) {
  console.log("Stopping PostgreSQL started by dev:landing...");
  runCompose(["stop", "db"]);
}

process.exitCode = exitCode;
