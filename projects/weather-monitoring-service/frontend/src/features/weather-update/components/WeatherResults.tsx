import type { WeatherUpdateResponse } from "../types";
import { BotActivationCard } from "./BotActivationCard";

type WeatherResultsProps = {
  isSubmitting: boolean;
  result: WeatherUpdateResponse | null;
};

function formatNumber(value: number) {
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(value);
}

export function WeatherResults({ isSubmitting, result }: WeatherResultsProps) {
  return (
    <section aria-live="polite" aria-busy={isSubmitting}>
      {result ? (
        <div className="animate-rise rounded-3xl border border-white/10 bg-slate-900/60 p-5 backdrop-blur-xl sm:p-7">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">
                Analysis complete
              </p>
              <h2 className="mt-2 text-2xl font-semibold text-white">{result.weather.location}</h2>
            </div>
            <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs text-slate-400">
              Normalized reading
            </span>
          </div>

          <dl className="mt-6 grid grid-cols-2 gap-3">
            <div className="rounded-2xl border border-white/[0.08] bg-white/[0.035] p-5">
              <dt className="text-xs font-medium uppercase tracking-wider text-slate-500">
                Temperature
              </dt>
              <dd className="mt-2 text-3xl font-semibold tracking-tight text-white">
                {formatNumber(result.weather.temperature)}
                <span className="ml-1 text-lg text-slate-500">°C</span>
              </dd>
            </div>
            <div className="rounded-2xl border border-white/[0.08] bg-white/[0.035] p-5">
              <dt className="text-xs font-medium uppercase tracking-wider text-slate-500">
                Humidity
              </dt>
              <dd className="mt-2 text-3xl font-semibold tracking-tight text-white">
                {formatNumber(result.weather.humidity)}
                <span className="ml-1 text-lg text-slate-500">%</span>
              </dd>
            </div>
          </dl>

          <div className="mt-6 border-t border-white/[0.08] pt-6">
            <h3 className="text-sm font-semibold text-slate-200">Bot activity</h3>
            {result.activations.length === 0 ? (
              <div className="mt-3 rounded-2xl border border-dashed border-white/10 px-5 py-6 text-center">
                <p className="text-sm font-medium text-slate-300">All conditions are calm</p>
                <p className="mt-1 text-xs text-slate-500">
                  No weather bots were activated by this reading.
                </p>
              </div>
            ) : (
              <ul className="mt-3 space-y-3">
                {result.activations.map((activation, index) => (
                  <BotActivationCard
                    key={`${activation.bot}-${index}`}
                    bot={activation.bot}
                    message={activation.message}
                  />
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : (
        <div className="rounded-3xl border border-dashed border-white/10 bg-white/[0.02] px-6 py-8 text-center">
          <p className="text-sm text-slate-500">
            {isSubmitting
              ? "Analyzing the weather observation…"
              : "Normalized readings and bot alerts will appear here."}
          </p>
        </div>
      )}
    </section>
  );
}
