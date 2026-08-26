import { z } from "zod";
import { apiErrorMessage } from "../../lib/api";
import { flightSchema, type Flight } from "../../lib/flightSchema";
import { bookingRecordSchema, type BookingRecord } from "../../types/booking";
export async function getMyBookings(id: string): Promise<BookingRecord[]> {
  const r = await fetch(
    `/api/bookings/me?passengerId=${encodeURIComponent(id)}`,
  );
  const d: unknown = await r.json();
  if (!r.ok) throw new Error(apiErrorMessage(d, "Could not load bookings"));
  return z.array(bookingRecordSchema).parse(d);
}
export async function getFlights(): Promise<Flight[]> {
  const r = await fetch("/api/flights");
  return z.array(flightSchema).parse(await r.json());
}
export async function cancelBooking(id: string, passengerId: string) {
  const r = await fetch(
    `/api/bookings/${id}/cancel?passengerId=${passengerId}`,
    { method: "POST" },
  );
  const d: unknown = await r.json();
  if (!r.ok) throw new Error(apiErrorMessage(d, "Could not cancel booking"));
  return d;
}
export async function modifyBooking(
  id: string,
  passengerId: string,
  flightId: string,
  className: string,
) {
  const r = await fetch(`/api/bookings/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ passengerId, flightId, class: className }),
  });
  const d: unknown = await r.json();
  if (!r.ok) throw new Error(apiErrorMessage(d, "Could not modify booking"));
  return bookingRecordSchema.parse(d);
}
