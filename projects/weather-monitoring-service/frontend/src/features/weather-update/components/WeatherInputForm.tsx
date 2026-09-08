import { useId } from "react";
import type { FormEvent } from "react";
import { weatherExamples } from "../examples";
import type { WeatherExampleFormat } from "../examples";

type WeatherInputFormProps = {
  error: string | null;
  format: WeatherExampleFormat;
  isSubmitting: boolean;
  rawData: string;
  onFormatChange: (format: WeatherExampleFormat) => void;
  onRawDataChange: (rawData: string) => void;
  onSubmit: () => Promise<void>;
};

export function WeatherInputForm({
  error,
  format,
  isSubmitting,
  rawData,
  onFormatChange,
  onRawDataChange,
  onSubmit,
}: WeatherInputFormProps) {
  const editorId = useId();

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void onSubmit();
  }

  const lineNumbers = rawData.split("\n").map((_, index) => String(index + 1).padStart(2, "0"));

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-3xl border border-white/10 bg-slate-900/70 p-5 shadow-2xl shadow-black/20 backdrop-blur-xl sm:p-7"
    >
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-white">Weather observation</h2>
          <p className="mt-1 text-sm text-slate-400">
            Paste raw station data or start with an example.
          </p>
        </div>
        <div
          role="group"
          className="inline-flex w-fit rounded-lg border border-white/10 bg-black/20 p-1"
          aria-label="Example format"
        >
          {(Object.keys(weatherExamples) as WeatherExampleFormat[]).map((exampleFormat) => (
            <button
              key={exampleFormat}
              type="button"
              onClick={() => onFormatChange(exampleFormat)}
              aria-pressed={format === exampleFormat}
              className={`rounded-md px-3 py-1.5 text-xs font-semibold transition focus:outline-none focus-visible:ring-2 focus-visible:ring-cyan-300 ${
                format === exampleFormat
                  ? "bg-slate-700 text-white shadow"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              {exampleFormat}
            </button>
          ))}
        </div>
      </div>

      <label htmlFor={editorId} className="sr-only">
        Raw weather data in JSON or XML format
      </label>
      <div className="relative mt-5 overflow-hidden">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute left-0 top-0 flex w-11 flex-col items-center border-r border-white/[0.06] pt-4 font-mono text-xs leading-6 text-slate-600"
        >
          {lineNumbers.map((lineNumber) => (
            <span key={lineNumber}>{lineNumber}</span>
          ))}
        </div>
        <textarea
          id={editorId}
          value={rawData}
          onChange={(event) => onRawDataChange(event.target.value)}
          spellCheck={false}
          rows={9}
          disabled={isSubmitting}
          aria-describedby={error ? `${editorId}-error` : undefined}
          aria-invalid={Boolean(error)}
          className="min-h-56 w-full resize-y rounded-2xl border border-white/10 bg-[#050b14]/90 py-4 pl-15 pr-4 font-mono text-[13px] leading-6 text-cyan-50 caret-cyan-300 outline-none transition placeholder:text-slate-600 focus:border-cyan-300/40 focus:ring-4 focus:ring-cyan-400/5 disabled:cursor-wait disabled:opacity-70"
        />
      </div>

      {error && (
        <div
          id={`${editorId}-error`}
          role="alert"
          className="mt-4 flex gap-3 rounded-xl border border-rose-400/20 bg-rose-400/10 px-4 py-3 text-sm leading-6 text-rose-100"
        >
          <span aria-hidden="true" className="font-bold text-rose-300">
            !
          </span>
          <span>{error}</span>
        </div>
      )}

      <div className="mt-5 flex flex-col-reverse gap-3 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-xs text-slate-500">
          Your input is processed as-is by the backend parsers.
        </p>
        <button
          type="submit"
          disabled={isSubmitting}
          className="inline-flex min-w-44 items-center justify-center gap-2 rounded-xl bg-cyan-300 px-5 py-3 text-sm font-bold text-slate-950 shadow-lg shadow-cyan-950/30 transition hover:bg-cyan-200 focus:outline-none focus-visible:ring-2 focus-visible:ring-cyan-200 focus-visible:ring-offset-2 focus-visible:ring-offset-slate-900 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isSubmitting ? (
            <>
              <span
                aria-hidden="true"
                className="h-4 w-4 animate-spin rounded-full border-2 border-slate-950/30 border-t-slate-950"
              />
              Analyzing…
            </>
          ) : (
            <>
              Analyze weather
              <span aria-hidden="true">→</span>
            </>
          )}
        </button>
      </div>
    </form>
  );
}
