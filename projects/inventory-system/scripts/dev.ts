export {};

function killProcessGroup(child: Bun.Subprocess, signal: NodeJS.Signals) {
  try {
    process.kill(-child.pid, signal);
  } catch {
    child.kill(signal);
  }
}

const processes = [
  Bun.spawn(["dotnet", "watch", "--no-hot-reload", "run", "--project", "backend/InventorySystem.csproj"], {
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: "Development" },
    detached: true,
  }),
  Bun.spawn(["bun", "run", "--no-orphans", "--cwd", "frontend", "dev"], {
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
