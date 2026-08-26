export {};

const projectDirectory = new URL("..", import.meta.url).pathname;
const backendProject = new URL("../backend/AirportTicketBookingSystem.csproj", import.meta.url).pathname;
const frontendDirectory = new URL("../frontend", import.meta.url).pathname;

const frontendPort = process.env.PROJECT_FRONTEND_PORT ?? "5173";

function killProcessGroup(child: Bun.Subprocess, signal: NodeJS.Signals) {
  try {
    process.kill(-child.pid, signal);
  } catch {
    child.kill(signal);
  }
}

const processes = [
  Bun.spawn(["dotnet", "watch", "--no-hot-reload", "run", "--project", backendProject], {
    cwd: projectDirectory,
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: "Development", ASPNETCORE_URLS: "http://localhost:5080" },
    detached: true,
  }),
  Bun.spawn(["bun", "run", "--no-orphans", "--cwd", frontendDirectory, "dev", "--", "--port", frontendPort, "--strictPort"], {
    cwd: projectDirectory,
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
    detached: true,
  }),
];

let stopping: Promise<void> | undefined;
function stop() {
  if (stopping) return stopping;
  stopping = (async () => {
    killProcessGroup(processes[0], "SIGINT");
    killProcessGroup(processes[1], "SIGTERM");

    const gracefulShutdown = Promise.allSettled(processes.map((process) => process.exited));
    const exitedGracefully = await Promise.race([
      gracefulShutdown.then(() => true),
      Bun.sleep(5000).then(() => false),
    ]);

    if (!exitedGracefully) {
      for (const process of processes) {
        if (process.exitCode === null) killProcessGroup(process, "SIGKILL");
      }
      await Promise.race([gracefulShutdown, Bun.sleep(2000)]);
    }
  })();
  return stopping;
}

process.once("SIGINT", () => void stop());
process.once("SIGTERM", () => void stop());

const firstExitCode = await Promise.race(processes.map((process) => process.exited));
await stop();
process.exitCode = firstExitCode;
