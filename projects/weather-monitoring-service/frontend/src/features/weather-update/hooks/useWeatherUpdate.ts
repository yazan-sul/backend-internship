import { useState } from "react";
import { publishWeatherUpdate, WeatherUpdateError } from "../api";
import { weatherExamples } from "../examples";
import type { WeatherExampleFormat } from "../examples";
import type { WeatherUpdateResponse } from "../types";

export function useWeatherUpdate() {
  const [format, setFormat] = useState<WeatherExampleFormat>("JSON");
  const [rawData, setRawDataState] = useState<string>(weatherExamples.JSON);
  const [result, setResult] = useState<WeatherUpdateResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function setRawData(value: string) {
    setRawDataState(value);
    setError(null);
  }

  function loadExample(nextFormat: WeatherExampleFormat) {
    setFormat(nextFormat);
    setRawDataState(weatherExamples[nextFormat]);
    setError(null);
  }

  async function submit() {
    if (!rawData.trim()) {
      setError("Enter a JSON or XML weather update before submitting.");
      return;
    }

    setIsSubmitting(true);
    setError(null);
    setResult(null);

    try {
      setResult(await publishWeatherUpdate(rawData));
    } catch (caughtError) {
      setError(
        caughtError instanceof WeatherUpdateError
          ? caughtError.message
          : "Something went wrong while publishing this update.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    error,
    format,
    isSubmitting,
    loadExample,
    rawData,
    result,
    setRawData,
    submit,
  };
}
