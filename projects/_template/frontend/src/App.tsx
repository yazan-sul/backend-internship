import { useState } from "react";
import { z } from "zod";

const messageSchema = z.object({ message: z.string().min(1) });

export function App() {
  const [message, setMessage] = useState("API not checked yet");

  async function checkApi() {
    try {
      const response = await fetch("/api/health");
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      setMessage(messageSchema.parse(await response.json()).message);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Unknown error");
    }
  }

  return (
    <main className="grid min-h-screen place-items-center bg-slate-950 px-6 text-slate-100">
      <section className="w-full max-w-xl rounded-2xl border border-slate-800 bg-slate-900 p-8 shadow-2xl">
        <p className="mb-2 text-sm font-semibold uppercase tracking-widest text-cyan-400">
          Internship project
        </p>
        <h1 className="text-4xl font-bold">Ready to build.</h1>
        <p className="mt-4 text-slate-300">
          React, TypeScript, Tailwind, Zod, ASP.NET Core, and raw PostgreSQL.
        </p>
        <button
          className="mt-8 rounded-lg bg-cyan-400 px-4 py-2 font-semibold text-slate-950 hover:bg-cyan-300"
          onClick={checkApi}
        >
          Check API
        </button>
        <p className="mt-3 text-sm text-slate-400" aria-live="polite">
          {message}
        </p>
      </section>
    </main>
  );
}
