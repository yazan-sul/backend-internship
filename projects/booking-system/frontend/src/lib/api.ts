export function apiErrorMessage(payload: unknown, fallback: string): string {
  if (typeof payload !== "object" || payload === null) return fallback;

  if ("message" in payload && typeof payload.message === "string") {
    return payload.message;
  }

  if ("title" in payload && typeof payload.title === "string") {
    return payload.title;
  }

  if ("errors" in payload && typeof payload.errors === "object" && payload.errors !== null) {
    const messages = Object.values(payload.errors as Record<string, unknown>)
      .flatMap((value) => (Array.isArray(value) ? value : []))
      .filter((value): value is string => typeof value === "string");
    if (messages.length > 0) return messages.join(" ");
  }

  return fallback;
}
