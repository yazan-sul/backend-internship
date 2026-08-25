import { useCallback, useEffect, useState } from "react";
import { z } from "zod";

const stack = ["React + TypeScript", "ASP.NET Core", "PostgreSQL", "Docker"];
const branches = __GIT_BRANCHES__;
const currentBranch = __CURRENT_BRANCH__;
const activeProject = __ACTIVE_PROJECT__;

const healthSchema = z.object({
  services: z.object({
    backend: z.enum(["healthy", "unavailable"]),
    postgresql: z.enum(["healthy", "unavailable"]),
  }),
});

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
  const initialState: ServiceState = activeProject ? "checking" : "unavailable";
  const [backend, setBackend] = useState<ServiceState>(initialState);
  const [postgresql, setPostgresql] = useState<ServiceState>(initialState);
  const [lastChecked, setLastChecked] = useState<string>();

  const checkHealth = useCallback(async () => {
    if (!activeProject) {
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
  }, []);

  useEffect(() => {
    void checkHealth();
  }, [checkHealth]);

  return (
    <main className="relative grid min-h-screen overflow-hidden bg-slate-950 px-6 py-16 text-white">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_right,rgba(34,211,238,0.16),transparent_35%),radial-gradient(circle_at_bottom_left,rgba(99,102,241,0.18),transparent_35%)]" />
      <section className="relative m-auto w-full max-w-5xl">
        <p className="text-sm font-bold uppercase tracking-[0.3em] text-cyan-300">Backend Internship</p>
        <h1 className="mt-6 max-w-4xl text-5xl font-black leading-tight sm:text-7xl">One foundation. A branch for every project.</h1>
        <p className="mt-7 max-w-2xl text-lg leading-8 text-slate-300">A focused workspace for learning production-minded backend development without coupling one project to the next.</p>
        <div className="mt-12 flex flex-wrap gap-3">
          {stack.map((item) => <span key={item} className="rounded-full border border-slate-700 bg-slate-900/80 px-4 py-2 text-sm text-slate-200">{item}</span>)}
        </div>
        <div className="mt-12 grid gap-6 lg:grid-cols-2">
          <section className="rounded-2xl border border-slate-700 bg-slate-950/60 p-5" aria-labelledby="branches-heading">
            <div className="flex items-start justify-between gap-4">
              <div>
                <h2 id="branches-heading" className="text-lg font-bold">Project branches</h2>
                <p className="mt-1 text-xs text-slate-400">Current branch: {currentBranch || "detached HEAD"}</p>
              </div>
              {activeProject ? (
                <a href={__PROJECT_URL__} className="rounded-lg bg-cyan-400 px-3 py-2 text-sm font-bold text-slate-950 transition hover:bg-cyan-300">
                  Open project
                </a>
              ) : (
                <button disabled className="cursor-not-allowed rounded-lg bg-slate-800 px-3 py-2 text-sm font-bold text-slate-500">
                  Open project
                </button>
              )}
            </div>
            <div className="mt-5 flex flex-wrap gap-2">
              {branches.length > 0 ? branches.map((branch) => (
                <span key={branch} className={`rounded-full border px-3 py-1.5 text-sm ${branch === currentBranch ? "border-cyan-400 bg-cyan-400/10 text-cyan-200" : "border-slate-700 bg-slate-900 text-slate-400"}`}>
                  {branch}
                </span>
              )) : <span className="text-sm text-slate-500">No local branches found.</span>}
            </div>
            <p className="mt-5 text-sm text-slate-400">
              {activeProject ? `${activeProject} is available at ${__PROJECT_URL__}.` : "Check out a branch with a matching project folder to enable its page."}
            </p>
          </section>
          <section className="rounded-2xl border border-slate-700 bg-slate-950/60 p-5" aria-labelledby="service-health-heading">
          <div className="mb-4 flex items-center justify-between gap-4">
            <div>
              <h2 id="service-health-heading" className="text-lg font-bold">Local service health</h2>
              <p className="mt-1 text-xs text-slate-400">
                {!activeProject ? "No active project on this branch" : lastChecked ? `Last checked at ${lastChecked}` : "Checking services…"}
              </p>
            </div>
            <button className="rounded-lg border border-cyan-400/60 px-3 py-2 text-sm font-semibold text-cyan-300 transition hover:bg-cyan-400/10 disabled:cursor-not-allowed disabled:opacity-60" onClick={() => void checkHealth()} disabled={!activeProject || backend === "checking" || postgresql === "checking"}>
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
