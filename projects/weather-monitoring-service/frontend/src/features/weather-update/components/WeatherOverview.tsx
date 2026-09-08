export function WeatherOverview() {
  return (
    <div>
      <div>
        <p className="mb-4 text-xs font-semibold uppercase tracking-[0.28em] text-cyan-300">
          Live weather station
        </p>
        <h1 className="max-w-3xl text-4xl font-semibold leading-[1.08] tracking-[-0.04em] text-white sm:text-5xl">
          Turn raw weather into clear signals.
        </h1>
        <p className="mt-4 max-w-2xl text-base leading-7 text-slate-400">
          Send a JSON or XML observation. Atmos normalizes the reading and alerts every bot whose
          conditions are met.
        </p>
      </div>

    </div>
  );
}
