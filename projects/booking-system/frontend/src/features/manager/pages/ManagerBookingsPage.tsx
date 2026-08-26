import { useEffect, useState } from "react";
import { date, money } from "../../../lib/formatters";
import type { BookingRecord } from "../../../types/booking";
import { searchManagerBookings } from "../api";
import { ManagerPageIntro } from "../components/ManagerPageIntro";

export function ManagerBookingsPage() {
  const [items, setItems] = useState<BookingRecord[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [searched, setSearched] = useState(false);

  async function search(event?: React.FormEvent<HTMLFormElement>) {
    event?.preventDefault();
    setLoading(true);
    setError("");
    try {
      const params = event
        ? new URLSearchParams(
            Array.from(new FormData(event.currentTarget).entries()) as [
              string,
              string,
            ][],
          )
        : new URLSearchParams();
      setItems(await searchManagerBookings(params));
      setSearched(true);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Could not load bookings");
      setItems([]);
    } finally {
      setLoading(false);
    }
  }
  useEffect(() => {
    void search();
  }, []);

  return (
    <div className="space-y-6">
      <ManagerPageIntro
        eyebrow="Bookings"
        title="Search booking records"
        description="Use the simple search first, or open advanced filters when you need a precise result."
      />
      <section className="card">
        <form onSubmit={search} className="grid gap-3 sm:grid-cols-2">
          <input name="flight" placeholder="Flight code" className="field" />
          <input
            name="passenger"
            placeholder="Passenger name or contact"
            className="field"
          />
          <details className="sm:col-span-2">
            <summary className="cursor-pointer text-sm font-semibold text-cyan-700">
              Advanced filters
            </summary>
            <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {[
                "departureCountry|From country",
                "destinationCountry|To country",
                "departureAirport|From airport",
                "arrivalAirport|To airport",
              ].map((item) => {
                const [name, placeholder] = item.split("|");
                return (
                  <input
                    key={name}
                    name={name}
                    placeholder={placeholder}
                    className="field"
                  />
                );
              })}
              <input name="date" type="date" className="field" />
              <select name="class" defaultValue="" className="field">
                <option value="">Any class</option>
                <option>Economy</option>
                <option>Business</option>
                <option>First</option>
              </select>
              <input
                name="minPrice"
                type="number"
                min="0"
                step="0.01"
                placeholder="Min booking price"
                className="field"
              />
              <input
                name="maxPrice"
                type="number"
                min="0"
                step="0.01"
                placeholder="Max booking price"
                className="field"
              />
            </div>
          </details>
          <button
            disabled={loading}
            className="rounded-lg bg-slate-950 px-4 py-3 font-semibold text-white transition hover:bg-slate-800 disabled:opacity-60 sm:col-span-2"
          >
            {loading ? "Loading bookings…" : "Search bookings"}
          </button>
        </form>
      </section>
      {error && <p className="text-sm text-red-600">{error}</p>}
      <BookingTable items={items} loading={loading} searched={searched} />
    </div>
  );
}

function BookingTable({
  items,
  loading,
  searched,
}: {
  items: BookingRecord[];
  loading: boolean;
  searched: boolean;
}) {
  return (
    <div className="overflow-x-auto rounded-2xl border border-slate-200 bg-white shadow-sm">
      <table className="w-full min-w-[760px] text-left text-sm">
        <thead className="bg-slate-50 text-slate-500">
          <tr>
            {[
              "Flight",
              "Passenger",
              "Class",
              "Price",
              "Departure",
              "Status",
            ].map((heading) => (
              <th className="p-4" key={heading}>
                {heading}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr className="border-t" key={item.booking.id}>
              <td className="p-4 font-semibold">{item.flight?.code}</td>
              <td className="p-4">
                {item.passenger?.name}
                <span className="block text-xs text-slate-400">
                  {item.passenger?.contactDetails}
                </span>
              </td>
              <td className="p-4">{item.booking.class}</td>
              <td className="p-4">{money(item.booking.finalPrice)}</td>
              <td className="p-4">
                {item.flight ? date(item.flight.departureAt) : "—"}
              </td>
              <td className="p-4">
                <span
                  className={`badge ${item.booking.status === "Active" ? "bg-emerald-100 text-emerald-700" : "bg-rose-100 text-rose-700"}`}
                >
                  {item.booking.status}
                </span>
              </td>
            </tr>
          ))}
          {items.length === 0 && !loading && (
            <tr>
              <td colSpan={6} className="p-10 text-center text-slate-500">
                {searched
                  ? "No bookings match these filters."
                  : "No bookings found."}
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
