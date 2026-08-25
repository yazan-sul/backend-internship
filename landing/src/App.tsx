import { useCallback, useEffect, useState } from "react";
import { z } from "zod";

const CONTROL_URL = import.meta.env.VITE_WORKSPACE_CONTROL_URL ?? "http://127.0.0.1:5090";
const pendingBranchKey = "backend-internship.pending-branch";
const localControlEnabled = import.meta.env.DEV;

const workspaceSchema = z.object({
  branches: z.array(z.object({ name: z.string(), hasProject: z.boolean() })),
  currentBranch: z.string(),
  activeProject: z.string().nullable(),
  dirty: z.boolean(),
  switchingTo: z.string().nullable(),
  projectReady: z.boolean(),
  landingReady: z.boolean(),
  projectUrl: z.string().url(),
  lastError: z.string().nullable(),
});

const healthSchema = z.object({
  services: z.object({
    backend: z.enum(["healthy", "unavailable"]),
    postgresql: z.enum(["healthy", "unavailable"]),
  }),
});

type Workspace = z.infer<typeof workspaceSchema>;
type Branch = Workspace["branches"][number];
type ServiceState = "checking" | "healthy" | "unavailable";

const stateStyles: Record<ServiceState, string> = {
  checking: "bg-amber-400",
  healthy: "bg-emerald-400",
  unavailable: "bg-rose-400",
};

function ServiceStatus({ name, state }: { name: string; state: ServiceState }) {
  return (
    <div className="flex items-center justify-between rounded-xl border border-slate-700 bg-slate-900/80 px-4 py-3">
      <span className="font-medium text-slate-200">{name}</span>
      <span className="flex items-center gap-2 text-sm capitalize text-slate-300">
        <span className={`h-2.5 w-2.5 rounded-full ${stateStyles[state]}`} />
        {state}
      </span>
    </div>
  );
}

export function App() {
  const [workspace, setWorkspace] = useState<Workspace>();
  const [workspaceError, setWorkspaceError] = useState<string>();
  const [backend, setBackend] = useState<ServiceState>("unavailable");
  const [postgresql, setPostgresql] = useState<ServiceState>("unavailable");
  const [lastChecked, setLastChecked] = useState<string>();

  const loadWorkspace = useCallback(async () => {
    if (!localControlEnabled) return;
    try {
      const response = await fetch(`${CONTROL_URL}/workspace/state`, {
        signal: AbortSignal.timeout(1500),
      });
      if (!response.ok) throw new Error("Workspace control is unavailable.");
      setWorkspace(workspaceSchema.parse(await response.json()));
      setWorkspaceError(undefined);
    } catch (error) {
      setWorkspaceError(error instanceof Error ? error.message : "Workspace control is unavailable.");
    }
  }, []);

  useEffect(() => {
    void loadWorkspace();
    const interval = window.setInterval(() => void loadWorkspace(), 10000);
    return () => window.clearInterval(interval);
  }, [loadWorkspace]);

  useEffect(() => {
    if (!workspace || workspace.switchingTo || !workspace.landingReady) return;
    const pendingBranch = sessionStorage.getItem(pendingBranchKey);
    if (!pendingBranch || workspace.currentBranch !== pendingBranch) return;

    const target = workspace.branches.find((branch) => branch.name === pendingBranch);
    if (target?.hasProject) {
      if (!workspace.projectReady || workspace.activeProject !== pendingBranch) return;
      sessionStorage.removeItem(pendingBranchKey);
      window.location.assign(workspace.projectUrl);
    } else {
      sessionStorage.removeItem(pendingBranchKey);
      window.location.reload();
    }
  }, [workspace]);

  const checkHealth = useCallback(async () => {
    if (!workspace?.activeProject) {
      setBackend("unavailable");
      setPostgresql("unavailable");
      return;
    }

    setBackend("checking");
    setPostgresql("checking");
    try {
      const response = await fetch("/api/health", { signal: AbortSignal.timeout(5000) });
      const health = healthSchema.parse(await response.json());
      setBackend(health.services.backend);
      setPostgresql(health.services.postgresql);
    } catch {
      setBackend("unavailable");
      setPostgresql("unavailable");
    } finally {
      setLastChecked(new Date().toLocaleTimeString());
    }
  }, [workspace?.activeProject]);

  useEffect(() => {
    void checkHealth();
  }, [checkHealth]);

  async function switchBranch(branch: Branch) {
    setWorkspaceError(undefined);
    sessionStorage.setItem(pendingBranchKey, branch.name);
    try {
      const response = await fetch(`${CONTROL_URL}/workspace/switch`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ branch: branch.name }),
      });
      const body = await response.json() as { error?: string };
      if (!response.ok) throw new Error(body.error ?? "Could not switch branches.");
      await loadWorkspace();
    } catch (error) {
      sessionStorage.removeItem(pendingBranchKey);
      setWorkspaceError(error instanceof Error ? error.message : "Could not switch branches.");
    }
  }

  const activeProject = workspace?.activeProject;
  const controlsDisabled = !workspace || workspace.dirty || Boolean(workspace.switchingTo);

  return (
    <main className="relative grid min-h-screen overflow-hidden bg-slate-950 px-6 py-16 text-white">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_right,rgba(34,211,238,0.16),transparent_35%),radial-gradient(circle_at_bottom_left,rgba(99,102,241,0.18),transparent_35%)]" />
      <section className="relative m-auto w-full max-w-5xl">
        <p className="text-sm font-bold uppercase tracking-[0.3em] text-cyan-300">Backend Internship</p>
        <h1 className="mt-6 max-w-4xl text-5xl font-black leading-tight sm:text-7xl">One foundation. A branch for every project.</h1>

        {(workspace?.dirty || workspaceError || workspace?.lastError) && (
          <div className="mt-8 rounded-xl border border-amber-400/40 bg-amber-400/10 px-4 py-3 text-sm text-amber-100" role="status">
            {workspace?.dirty
              ? "Branch switching is locked because the worktree has uncommitted changes. Commit or stash them first."
              : workspaceError ?? workspace?.lastError}
          </div>
        )}

        <div className="mt-12 grid gap-6 lg:grid-cols-2">
          <section className="rounded-2xl border border-slate-700 bg-slate-950/60 p-5" aria-labelledby="branches-heading">
            <div>
              <h2 id="branches-heading" className="text-lg font-bold">Project branches</h2>
              <p className="mt-1 text-xs text-slate-400">
                Current branch: {localControlEnabled ? workspace?.currentBranch || "Loading…" : "Development only"}
              </p>
            </div>

            <div className="mt-5 space-y-2">
              {workspace?.branches.map((branch) => {
                const isCurrent = branch.name === workspace.currentBranch;
                return (
                  <div key={branch.name} className={`flex items-center justify-between gap-3 rounded-xl border px-3 py-2 ${isCurrent ? "border-cyan-400 bg-cyan-400/10" : "border-slate-700 bg-slate-900/80"}`}>
                    <div>
                      <p className={isCurrent ? "font-semibold text-cyan-200" : "font-medium text-slate-300"}>{branch.name}</p>
                      <p className="text-xs text-slate-500">{branch.hasProject ? "Project branch" : "Scaffold branch"}</p>
                    </div>
                    {isCurrent && activeProject ? (
                      <a href={workspace.projectUrl} className="rounded-lg bg-cyan-400 px-3 py-2 text-xs font-bold text-slate-950 hover:bg-cyan-300">Open project</a>
                    ) : isCurrent ? (
                      <span className="px-3 py-2 text-xs font-semibold text-slate-500">Current</span>
                    ) : (
                      <button
                        disabled={controlsDisabled}
                        onClick={() => void switchBranch(branch)}
                        className="rounded-lg border border-cyan-400/50 px-3 py-2 text-xs font-bold text-cyan-300 hover:bg-cyan-400/10 disabled:cursor-not-allowed disabled:border-slate-700 disabled:text-slate-600"
                      >
                        {workspace.switchingTo === branch.name ? "Switching…" : branch.hasProject ? "Switch & open" : "Switch"}
                      </button>
                    )}
                  </div>
                );
              })}
              {!workspace && (
                <p className="text-sm text-slate-500">
                  {localControlEnabled ? "Connecting to local workspace control…" : "Branch controls are disabled in production."}
                </p>
              )}
            </div>
          </section>

          <section className="rounded-2xl border border-slate-700 bg-slate-950/60 p-5" aria-labelledby="service-health-heading">
            <div className="mb-4 flex items-center justify-between gap-4">
              <div>
                <h2 id="service-health-heading" className="text-lg font-bold">Local service health</h2>
                <p className="mt-1 text-xs text-slate-400">
                  {!activeProject ? "No active project on this branch" : lastChecked ? `Last checked at ${lastChecked}` : "Checking services…"}
                </p>
              </div>
              <button className="rounded-lg border border-cyan-400/60 px-3 py-2 text-sm font-semibold text-cyan-300 hover:bg-cyan-400/10 disabled:cursor-not-allowed disabled:opacity-60" onClick={() => void checkHealth()} disabled={!activeProject || backend === "checking" || postgresql === "checking"}>
                Check again
              </button>
            </div>
            <div className="grid gap-3 sm:grid-cols-2" aria-live="polite">
              <ServiceStatus name="Backend API" state={backend} />
              <ServiceStatus name="PostgreSQL" state={postgresql} />
            </div>
          </section>
        </div>
      </section>
    </main>
  );
}
