export {};

const frontendPort = process.env.PROJECT_FRONTEND_PORT ?? "5173";

const processes = [
  Bun.spawn(["dotnet", "watch", "--no-hot-reload", "run", "--project", "backend/ProjectTemplate.csproj"], {
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: "Development" },
  }),
  Bun.spawn(["bun", "run", "--no-orphans", "--cwd", "frontend", "dev", "--", "--port", frontendPort, "--strictPort"], {
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
  }),
];

let stopping: Promise<void> | undefined;
function stop() {
  if (stopping) return stopping;
  stopping = (async () => {
    processes[0].kill("SIGINT");
    processes[1].kill("SIGTERM");

    const gracefulShutdown = Promise.allSettled(processes.map((process) => process.exited));
    const exitedGracefully = await Promise.race([
      gracefulShutdown.then(() => true),
      Bun.sleep(5000).then(() => false),
    ]);

    if (!exitedGracefully) {
      for (const process of processes) {
        if (process.exitCode === null) process.kill("SIGKILL");
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
