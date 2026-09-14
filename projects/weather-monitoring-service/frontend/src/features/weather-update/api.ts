import { apiErrorResponseSchema, weatherUpdateResponseSchema } from "./types";
import type { WeatherUpdateResponse } from "./types";

export class WeatherUpdateError extends Error {}

export async function publishWeatherUpdate(rawData: string): Promise<WeatherUpdateResponse> {
  let response: Response;

  try {
    response = await fetch("/api/weather-updates", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ rawData }),
    });
  } catch {
    throw new WeatherUpdateError(
      "We couldn't reach the weather service. Check that the backend is running and try again.",
    );
  }

  const payload: unknown = await response.json().catch(() => null);

  if (!response.ok) {
    const parsedError = apiErrorResponseSchema.safeParse(payload);
    throw new WeatherUpdateError(
      parsedError.success
        ? parsedError.data.error
        : "The weather service couldn't process this update.",
    );
  }

  const parsedResponse = weatherUpdateResponseSchema.safeParse(payload);
  if (!parsedResponse.success) {
    throw new WeatherUpdateError("The weather service returned an unexpected response.");
  }

  return parsedResponse.data;
}
