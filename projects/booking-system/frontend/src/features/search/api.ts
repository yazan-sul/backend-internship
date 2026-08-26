import { z } from "zod";
import { apiErrorMessage } from "../../lib/api";
import { flightSchema, type Flight } from "../../lib/flightSchema";
import { bookingSchema } from "../../types/booking";

export async function searchFlights(
  params = new URLSearchParams(),
): Promise<Flight[]> {
  const response = await fetch(`/api/flights?${params}`);
  const payload: unknown = await response.json();
  if (!response.ok)
    throw new Error(apiErrorMessage(payload, "Could not load flights"));
  return z.array(flightSchema).parse(payload);
}
export async function createPassenger(
  name: string,
  contactDetails: string,
): Promise<string> {
  const response = await fetch("/api/passengers", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, contactDetails }),
  });
  const payload: unknown = await response.json();
  if (
    !response.ok ||
    typeof payload !== "object" ||
    payload === null ||
    !("id" in payload) ||
    typeof payload.id !== "string"
  )
    throw new Error(
      apiErrorMessage(payload, "Could not create passenger identity"),
    );
  return payload.id;
}
export async function createBooking(
  passengerId: string,
  flightId: string,
  className: string,
) {
  const response = await fetch("/api/bookings", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ passengerId, flightId, class: className }),
  });
  const payload: unknown = await response.json();
  if (!response.ok)
    throw new Error(apiErrorMessage(payload, "Could not create booking"));
  return z.object({ booking: bookingSchema }).passthrough().parse(payload);
}
