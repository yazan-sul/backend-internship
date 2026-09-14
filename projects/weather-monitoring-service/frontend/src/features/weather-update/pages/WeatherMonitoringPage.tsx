import { WeatherInputForm } from "../components/WeatherInputForm";
import { WeatherOverview } from "../components/WeatherOverview";
import { WeatherResults } from "../components/WeatherResults";
import { useWeatherUpdate } from "../hooks/useWeatherUpdate";

export function WeatherMonitoringPage() {
  const weatherUpdate = useWeatherUpdate();

  return (
    <section className="flex flex-1 flex-col gap-8 py-8 lg:gap-10 lg:py-12">
      <WeatherOverview />

      <div className="grid gap-5 lg:grid-cols-2 lg:items-start lg:gap-6">
        <WeatherInputForm
          error={weatherUpdate.error}
          format={weatherUpdate.format}
          isSubmitting={weatherUpdate.isSubmitting}
          rawData={weatherUpdate.rawData}
          onFormatChange={weatherUpdate.loadExample}
          onRawDataChange={weatherUpdate.setRawData}
          onSubmit={weatherUpdate.submit}
        />
        <WeatherResults
          isSubmitting={weatherUpdate.isSubmitting}
          result={weatherUpdate.result}
        />
      </div>
    </section>
  );
}
