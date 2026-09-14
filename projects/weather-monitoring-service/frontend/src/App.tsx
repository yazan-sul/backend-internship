import { WeatherMonitoringPage } from "./features/weather-update/pages/WeatherMonitoringPage";

export function App() {
  return (
    <main className="relative min-h-screen overflow-hidden bg-[#102a43] text-slate-100">
      <div aria-hidden="true" className="pointer-events-none absolute inset-0 overflow-hidden">
        <div className="absolute -left-40 top-24 h-96 w-96 rounded-full bg-cyan-400/10 blur-3xl" />
        <div className="absolute -right-32 -top-32 h-[32rem] w-[32rem] rounded-full bg-blue-500/10 blur-3xl" />
        <div className="weather-grid absolute inset-0 opacity-30" />
      </div>

      <div className="relative mx-auto flex min-h-screen w-full max-w-7xl flex-col px-5 sm:px-8 lg:px-12">
        <WeatherMonitoringPage />

        <footer className="border-t border-white/10 py-5 text-xs text-slate-600">
          Strategy-powered parsing · Observer-powered alerts
        </footer>
      </div>
    </main>
  );
}
