export {};

const processes = [
  Bun.spawn(["dotnet", "watch", "--no-hot-reload", "run", "--project", "backend/InventorySystem.csproj"], {
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: "Development" },
  }),
  Bun.spawn(["bun", "run", "--cwd", "frontend", "dev"], {
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit",
  }),
];

let stopping = false;
function stop() {
  if (stopping) return;
  stopping = true;
  for (const process of processes) process.kill("SIGTERM");
}

process.once("SIGINT", stop);
process.once("SIGTERM", stop);

const firstExitCode = await Promise.race(processes.map((process) => process.exited));
stop();
await Promise.allSettled(processes.map((process) => process.exited));
process.exitCode = firstExitCode;
