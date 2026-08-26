import { useEffect, useState } from "react";
import { date, money } from "../../lib/formatters";
import type { Flight } from "../../lib/flightSchema";
import { bookingSchema, type BookingRecord } from "../../types/booking";
import { cancelBooking, getFlights, getMyBookings, modifyBooking } from "./api";
export function BookingsPage({ passengerId }: { passengerId: string | null }) {
  const [items, setItems] = useState<BookingRecord[]>([]);
  const [flights, setFlights] = useState<Flight[]>([]);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [flightId, setFlightId] = useState("");
  const [className, setClassName] = useState("Economy");
  const [message, setMessage] = useState("");
  useEffect(() => {
    if (!passengerId) {
      setItems([]);
      return;
    }
    void Promise.all([getMyBookings(passengerId), getFlights()])
      .then(([bookings, available]) => {
        setItems(bookings);
        setFlights(available);
      })
      .catch(() => setItems([]));
  }, [passengerId]);
  async function cancel(id: string) {
    if (!passengerId || !window.confirm("Cancel this booking?")) return;
    try {
      const booking = bookingSchema.parse(await cancelBooking(id, passengerId));
      setItems((current) =>
        current.map((item) =>
          item.booking.id === id
            ? { ...item, booking }
            : item,
        ),
      );
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Could not cancel booking");
    }
  }
  async function modify(id: string) {
    if (!passengerId || !flightId) return;
    try {
      const next = await modifyBooking(id, passengerId, flightId, className);
      setItems((current) =>
        current.map((item) => (item.booking.id === id ? next : item)),
      );
      setEditingId(null);
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Could not modify booking");
    }
  }
  return (
    <section>
      <h2 className="heading">My bookings</h2>
      {message && <p className="mt-4 text-red-600">{message}</p>}
      <div className="mt-6 grid gap-3">
        {items.length === 0 ? (
          <div className="card text-slate-500">
            No bookings for this passenger yet.
          </div>
        ) : (
          items.map((item) => (
            <div className="card" key={item.booking.id}>
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="font-bold">
                    {item.flight?.code} · {item.booking.class}
                  </p>
                  <p className="text-sm text-slate-500">
                    {item.flight?.departureAirport} →{" "}
                    {item.flight?.arrivalAirport} ·{" "}
                    {money(item.booking.finalPrice)}
                  </p>
                </div>
                <span
                  className={`badge ${item.booking.status === "Active" ? "bg-emerald-100 text-emerald-700" : "bg-slate-100 text-slate-500"}`}
                >
                  {item.booking.status}
                </span>
              </div>
              {item.booking.status === "Active" && (
                <div className="mt-4 flex flex-wrap gap-2">
                  <button
                    onClick={() => void cancel(item.booking.id)}
                    className="rounded-lg border border-red-200 px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50"
                  >
                    Cancel booking
                  </button>
                  {editingId === item.booking.id ? (
                    <>
                      <select
                        value={flightId}
                        onChange={(e) => setFlightId(e.target.value)}
                        className="field"
                      >
                        <option value="">Choose flight</option>
                        {flights.map((flight) => (
                          <option key={flight.id} value={flight.id}>
                            {flight.code} · {flight.departureAirport} →{" "}
                            {flight.arrivalAirport}
                          </option>
                        ))}
                      </select>
                      <select
                        value={className}
                        onChange={(e) => setClassName(e.target.value)}
                        className="field"
                      >
                        <option>Economy</option>
                        <option>Business</option>
                        <option>First</option>
                      </select>
                      <button
                        onClick={() => void modify(item.booking.id)}
                        className="rounded-lg bg-cyan-600 px-3 py-2 text-sm font-semibold text-white"
                      >
                        Save changes
                      </button>
                      <button
                        onClick={() => setEditingId(null)}
                        className="rounded-lg border px-3 py-2 text-sm"
                      >
                        Close
                      </button>
                    </>
                  ) : (
                    <button
                      onClick={() => {
                        setEditingId(item.booking.id);
                        setFlightId(item.booking.flightId);
                        setClassName(item.booking.class);
                      }}
                      className="rounded-lg border border-cyan-200 px-3 py-2 text-sm font-semibold text-cyan-700 hover:bg-cyan-50"
                    >
                      Modify booking
                    </button>
                  )}
                </div>
              )}
            </div>
          ))
        )}
      </div>
    </section>
  );
}
