import { useState } from "react";
import type { ImportResult } from "../../../types/booking";
import { importFlights } from "../api";
import { ManagerPageIntro } from "../components/ManagerPageIntro";

export function ManagerImportPage() {
  const [file, setFile] = useState<File | null>(null);
  const [result, setResult] = useState<ImportResult | null>(null);
  const [importing, setImporting] = useState(false);
  const [error, setError] = useState("");
  async function onImport(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!file) return;
    setImporting(true);
    setResult(null);
    setError("");
    try {
      const next = await importFlights(file);
      setResult(next);
      if (!next.errors?.length) setFile(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Could not import flights");
    } finally {
      setImporting(false);
    }
  }
  return (
    <div className="max-w-3xl space-y-6">
      <ManagerPageIntro
        eyebrow="Flight inventory"
        title="Import flights"
        description="Upload a CSV file to add new flight inventory to the system."
      />
      <form onSubmit={onImport} className="card border-dashed border-cyan-200">
        <div className="rounded-xl bg-cyan-50 p-5 text-sm text-cyan-900">
          <p className="font-bold">Before you upload</p>
          <p className="mt-1 leading-6">
            Use the documented 12-column CSV format. Validation runs
            automatically and rejected rows will be listed below.
          </p>
        </div>
        <label className="mt-6 block text-sm font-semibold text-slate-700">
          Flight CSV file
        </label>
        <input
          type="file"
          accept=".csv,text/csv"
          onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          className="field mt-2 w-full"
        />
        <button
          disabled={!file || importing}
          className="mt-4 w-full rounded-lg bg-cyan-600 px-4 py-3 font-semibold text-white transition hover:bg-cyan-700 disabled:opacity-50"
        >
          {importing ? "Importing…" : "Import CSV"}
        </button>
        {error && <p className="mt-4 text-sm text-red-600">{error}</p>}
        {result && (
          <div
            className={`mt-4 rounded-xl p-4 text-sm ${result.errors?.length ? "bg-amber-50 text-amber-900" : "bg-emerald-50 text-emerald-800"}`}
          >
            <p className="font-semibold">
              {result.errors?.length
                ? "Import rejected — no flights were added."
                : `${result.imported ?? 0} flights imported successfully.`}
            </p>
            {result.errors?.map((item, index) => (
              <p key={`${item.row}-${item.field}-${index}`}>
                <strong>
                  Row {item.row}, {item.field}:
                </strong>{" "}
                {item.message}
                {item.value ? ` (${item.value})` : ""}
              </p>
            ))}
          </div>
        )}
      </form>
    </div>
  );
}
