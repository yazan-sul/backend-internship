import { useState } from "react";
import { ManagerBookingsPage } from "./pages/ManagerBookingsPage";
import { ManagerImportPage } from "./pages/ManagerImportPage";
import { ManagerOverviewPage } from "./pages/ManagerOverviewPage";
import { ManagerValidationPage } from "./pages/ManagerValidationPage";

export type ManagerView = "overview" | "bookings" | "import" | "validation";

const views: { id: ManagerView; label: string; description: string }[] = [
  { id: "overview", label: "Overview", description: "At a glance" },
  { id: "bookings", label: "Bookings", description: "Search and review" },
  { id: "import", label: "Import flights", description: "Upload CSV" },
  { id: "validation", label: "Validation", description: "Flight rules" },
];

export function ManagerPage() {
  const [view, setView] = useState<ManagerView>("overview");
  return (
    <section className="space-y-8">
      <header>
        <p className="font-semibold text-cyan-700">Operations</p>
        <h2 className="heading mt-1">Manager workspace</h2>
        <p className="mt-2 max-w-2xl text-slate-500">
          Keep bookings, flight imports, and validation rules organized in one simple workspace.
        </p>
      </header>
      <nav className="grid gap-2 rounded-2xl border border-slate-200 bg-white p-2 shadow-sm sm:grid-cols-4">
        {views.map((item) => (
          <button key={item.id} onClick={() => setView(item.id)} className={`rounded-xl px-4 py-3 text-left transition ${view === item.id ? "bg-slate-950 text-white shadow-md" : "text-slate-600 hover:bg-slate-50"}`}>
            <span className="block text-sm font-bold">{item.label}</span>
            <span className={`mt-1 block text-xs ${view === item.id ? "text-slate-300" : "text-slate-400"}`}>{item.description}</span>
          </button>
        ))}
      </nav>
      {view === "overview" && <ManagerOverviewPage onNavigate={setView} />}
      {view === "bookings" && <ManagerBookingsPage />}
      {view === "import" && <ManagerImportPage />}
      {view === "validation" && <ManagerValidationPage />}
    </section>
  );
}
