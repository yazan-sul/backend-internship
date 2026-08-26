import { useEffect, useState } from "react";
import { z } from "zod";

const flightSchema = z.object({
  id: z.string(),
  code: z.string(),
  departureCountry: z.string(),
  destinationCountry: z.string(),
  departureAirport: z.string(),
  arrivalAirport: z.string(),
  departureAt: z.string(),
  prices: z.object({
    economy: z.number(),
    business: z.number(),
    first: z.number(),
  }),
  availability: z.object({
    economy: z.number(),
    business: z.number(),
    first: z.number(),
  }),
});
type Flight = z.infer<typeof flightSchema>;
type Tab = "search" | "bookings" | "manager";
const money = (n: number) =>
  new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(
    n,
  );
const date = (s: string) =>
  new Date(s).toLocaleString([], { dateStyle: "medium", timeStyle: "short" });
const passengerCookie = "skybook.passengerId";
const readCookie = (name: string) => {
  const value = document.cookie
    .split("; ")
    .find((cookie) => cookie.startsWith(`${name}=`))
    ?.slice(name.length + 1);
  return value ? decodeURIComponent(value) : null;
};
const writeCookie = (name: string, value: string) => {
  const secure = window.location.protocol === "https:" ? "; Secure" : "";
  document.cookie = `${name}=${encodeURIComponent(value)}; Max-Age=31536000; Path=/; SameSite=Lax${secure}`;
};
const removeCookie = (name: string) => {
  document.cookie = `${name}=; Max-Age=0; Path=/; SameSite=Lax`;
};

export function App() {
  const [tab, setTab] = useState<Tab>("search");
  const [flights, setFlights] = useState<Flight[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [passengerName, setPassengerName] = useState("Demo Passenger");
  const [contactDetails, setContactDetails] = useState("demo@skybook.test");
  const [passengerId, setPassengerId] = useState<string | null>(() =>
    readCookie(passengerCookie),
  );
  const [notice, setNotice] = useState("");
  const search = async (event?: React.FormEvent) => {
    event?.preventDefault();
    setError("");
    setLoading(true);
    try {
      const form = event?.currentTarget as HTMLFormElement | undefined;
      const params = form
        ? new URLSearchParams(
            Array.from(new FormData(form).entries()) as [string, string][],
          )
        : new URLSearchParams();
      const response = await fetch(`/api/flights?${params}`);
      const payload: unknown = await response.json();
      if (!response.ok) {
        if (
          typeof payload === "object" &&
          payload !== null &&
          "errors" in payload &&
          typeof payload.errors === "object" &&
          payload.errors !== null
        ) {
          const messages = Object.values(payload.errors as Record<string, unknown>)
            .flatMap((value) => (Array.isArray(value) ? value : []))
            .filter((value): value is string => typeof value === "string");
          throw new Error(messages.join(" ") || "Could not load flights");
        }
        throw new Error("Could not load flights");
      }
      setFlights(z.array(flightSchema).parse(payload));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Unexpected error");
    } finally {
      setLoading(false);
    }
  };
  useEffect(() => {
    void search();
  }, []);
  async function ensurePassenger() {
    if (passengerId) return passengerId;

    const response = await fetch("/api/passengers", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        name: passengerName,
        contactDetails,
      }),
    });
    const data = await response.json();
    if (!response.ok || typeof data.id !== "string") {
      throw new Error(data.message ?? "Could not create passenger identity");
    }

    writeCookie(passengerCookie, data.id);
    setPassengerId(data.id);
    return data.id;
  }

  async function book(flight: Flight, cls: string, price: number) {
    setNotice("");
    try {
      const id = await ensurePassenger();
      const response = await fetch("/api/bookings", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          passengerId: id,
          flightId: flight.id,
          class: cls,
        }),
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message ?? "Could not create booking");

      setNotice(
        `Booking confirmed: ${data.booking.id} · ${flight.code} · ${cls} · ${money(price)} · ${date(flight.departureAt)}`,
      );
      void search();
    } catch (e) {
      setNotice(e instanceof Error ? e.message : "Could not create booking");
    }
  }
  return (
    <div className="min-h-screen bg-[#f5f7fb] text-slate-900">
      <header className="bg-slate-950 text-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-5">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.25em] text-cyan-300">
              SkyBook
            </p>
            <h1 className="mt-1 text-2xl font-bold">Travel farther, simply.</h1>
          </div>
          <label className="text-sm text-slate-300">
            Passenger details{" "}
            <input
              value={passengerName}
              onChange={(e) => {
                removeCookie(passengerCookie);
                setPassengerId(null);
                setPassengerName(e.target.value);
              }}
              placeholder="Name"
              className="ml-2 w-36 rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white"
            />
            <input
              value={contactDetails}
              onChange={(e) => {
                removeCookie(passengerCookie);
                setPassengerId(null);
                setContactDetails(e.target.value);
              }}
              placeholder="Contact details"
              className="ml-2 rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white"
            />
          </label>
        </div>
      </header>
      <nav className="border-b bg-white">
        <div className="mx-auto flex max-w-6xl gap-6 px-6">
          {(["search", "bookings", "manager"] as Tab[]).map((x) => (
            <button
              key={x}
              onClick={() => setTab(x)}
              className={`border-b-2 py-4 text-sm font-semibold capitalize ${tab === x ? "border-cyan-500 text-cyan-700" : "border-transparent text-slate-500"}`}
            >
              {x === "search" ? "Find a flight" : x}
            </button>
          ))}
        </div>
      </nav>
      <main className="mx-auto max-w-6xl px-6 py-10">
        {notice && (
          <div className="mb-6 rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-emerald-800">
            {notice}
          </div>
        )}
        {tab === "search" && (
          <>
            <section className="mb-8">
              <p className="font-semibold text-cyan-700">Your next departure</p>
              <h2 className="mt-2 text-4xl font-bold tracking-tight">
                Where will you go?
              </h2>
              <p className="mt-2 text-slate-500">
                Search live inventory and reserve the class that fits your
                journey.
              </p>
            </section>
            <form
              onSubmit={search}
              className="grid gap-3 rounded-2xl bg-white p-5 shadow-sm sm:grid-cols-5"
            >
              <input
                name="departureCountry"
                placeholder="From country"
                className="field"
              />
              <input
                name="destinationCountry"
                placeholder="To country"
                className="field"
              />
              <input
                name="departureAirport"
                placeholder="From airport"
                className="field"
              />
              <input
                name="arrivalAirport"
                placeholder="To airport"
                className="field"
              />
              <input name="date" type="date" className="field" />
              <input
                name="minPrice"
                type="number"
                min="0"
                step="0.01"
                placeholder="Min price"
                className="field"
              />
              <input
                name="maxPrice"
                type="number"
                min="0"
                step="0.01"
                placeholder="Max price"
                className="field"
              />
              <select name="class" defaultValue="" className="field">
                <option value="">Any class</option>
                <option value="Economy">Economy</option>
                <option value="Business">Business</option>
                <option value="First">First</option>
              </select>
              <button
                disabled={loading}
                className="rounded-lg bg-cyan-600 px-4 py-3 font-semibold text-white hover:bg-cyan-700 disabled:cursor-wait disabled:opacity-60 sm:col-span-5"
              >
                {loading ? "Searching…" : "Search flights"}
              </button>
            </form>
            {error && <p className="mt-4 text-red-600">{error}</p>}
            <div className="mt-8 grid gap-4">
              {flights.length === 0 ? (
                <div className="rounded-2xl bg-white p-10 text-center text-slate-500">
                  No matching flights. Try widening your search.
                </div>
              ) : (
                flights.map((f) => (
                  <article
                    key={f.id}
                    className="rounded-2xl bg-white p-5 shadow-sm"
                  >
                    <div className="flex flex-wrap items-center justify-between gap-4">
                      <div>
                        <p className="text-sm font-bold text-cyan-700">
                          {f.code}
                        </p>
                        <h3 className="mt-1 text-xl font-bold">
                          {f.departureAirport}{" "}
                          <span className="text-slate-300">→</span>{" "}
                          {f.arrivalAirport}
                        </h3>
                        <p className="text-sm text-slate-500">
                          {f.departureCountry} to {f.destinationCountry} ·{" "}
                          {date(f.departureAt)}
                        </p>
                      </div>
                      <div className="grid grid-cols-3 gap-2">
                        {(["economy", "business", "first"] as const).map(
                          (cls) => (
                            <button
                              disabled={!f.availability[cls]}
                              key={cls}
                              onClick={() =>
                                void book(
                                  f,
                                  cls[0].toUpperCase() + cls.slice(1),
                                  f.prices[cls],
                                )
                              }
                              className="rounded-lg border border-slate-200 px-3 py-2 text-left text-xs hover:border-cyan-400 disabled:cursor-not-allowed disabled:opacity-40"
                            >
                              <span className="block capitalize text-slate-500">
                                {cls}
                              </span>
                              <strong className="text-sm">
                                {money(f.prices[cls])}
                              </strong>
                              <span className="block text-slate-400">
                                {f.availability[cls]} seats
                              </span>
                            </button>
                          ),
                        )}
                      </div>
                    </div>
                  </article>
                ))
              )}
            </div>
          </>
        )}
        {tab === "bookings" && <Bookings passengerId={passengerId} />}
        {tab === "manager" && <Manager />}
      </main>
    </div>
  );
}
function Bookings({ passengerId }: { passengerId: string | null }) {
  const [items, setItems] = useState<any[]>([]);
  const [availableFlights, setAvailableFlights] = useState<Flight[]>([]);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editFlightId, setEditFlightId] = useState("");
  const [editClass, setEditClass] = useState("Economy");
  const [message, setMessage] = useState("");
  useEffect(() => {
    if (!passengerId) {
      setItems([]);
      return;
    }
    fetch(`/api/bookings/me?passengerId=${encodeURIComponent(passengerId)}`)
      .then((r) => (r.ok ? r.json() : Promise.reject(new Error("Could not load bookings"))))
      .then(setItems)
      .catch(() => setItems([]));
    fetch("/api/flights")
      .then((r) => r.json())
      .then((data) => setAvailableFlights(z.array(flightSchema).parse(data)))
      .catch(() => setAvailableFlights([]));
  }, [passengerId]);

  async function cancel(id: string) {
    if (!passengerId || !window.confirm("Cancel this booking?")) return;
    setMessage("");
    const response = await fetch(`/api/bookings/${id}/cancel?passengerId=${passengerId}`, {
      method: "POST",
    });
    const data = await response.json();
    if (!response.ok) {
      setMessage(data.message ?? "Could not cancel booking");
      return;
    }
    setItems((current) =>
      current.map((item) =>
        item.booking.id === id ? { ...item, booking: data } : item,
      ),
    );
  }

  async function modify(id: string) {
    if (!passengerId || !editFlightId) return;
    setMessage("");
    const response = await fetch(`/api/bookings/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        passengerId,
        flightId: editFlightId,
        class: editClass,
      }),
    });
    const data = await response.json();
    if (!response.ok) {
      setMessage(data.message ?? "Could not modify booking");
      return;
    }
    setItems((current) =>
      current.map((item) => (item.booking.id === id ? data : item)),
    );
    setEditingId(null);
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
          items.map((x) => (
            <div className="card" key={x.booking.id}>
              <div className="flex items-center justify-between gap-4">
              <div>
                <p className="font-bold">
                  {x.flight?.code} · {x.booking.class}
                </p>
                <p className="text-sm text-slate-500">
                  {x.flight?.departureAirport} → {x.flight?.arrivalAirport} ·{" "}
                  {money(x.booking.finalPrice)}
                </p>
              </div>
              <span
                className={`badge ${x.booking.status === "Active" ? "bg-emerald-100 text-emerald-700" : "bg-slate-100 text-slate-500"}`}
              >
                {x.booking.status}
              </span>
              </div>
              {x.booking.status === "Active" && (
                <div className="mt-4 flex flex-wrap gap-2">
                  <button
                    onClick={() => void cancel(x.booking.id)}
                    className="rounded-lg border border-red-200 px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50"
                  >
                    Cancel booking
                  </button>
                  {editingId === x.booking.id ? (
                    <>
                      <select
                        value={editFlightId}
                        onChange={(event) => setEditFlightId(event.target.value)}
                        className="field"
                      >
                        <option value="">Choose flight</option>
                        {availableFlights.map((flight) => (
                          <option key={flight.id} value={flight.id}>
                            {flight.code} · {flight.departureAirport} → {flight.arrivalAirport}
                          </option>
                        ))}
                      </select>
                      <select
                        value={editClass}
                        onChange={(event) => setEditClass(event.target.value)}
                        className="field"
                      >
                        <option>Economy</option>
                        <option>Business</option>
                        <option>First</option>
                      </select>
                      <button
                        onClick={() => void modify(x.booking.id)}
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
                        setEditingId(x.booking.id);
                        setEditFlightId(x.booking.flightId);
                        setEditClass(x.booking.class);
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
function Manager() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [hasSearched, setHasSearched] = useState(false);

  async function search(event?: React.FormEvent) {
    event?.preventDefault();
    setLoading(true);
    setError("");
    try {
      const form = event?.currentTarget as HTMLFormElement | undefined;
      const params = form
        ? new URLSearchParams(
            Array.from(new FormData(form).entries()) as [string, string][],
          )
        : new URLSearchParams();
      const response = await fetch(`/api/manager/bookings?${params}`);
      const data = await response.json();
      if (!response.ok) {
        const messages = Object.values(data.errors ?? {})
          .flatMap((value) => (Array.isArray(value) ? value : []))
          .filter((value): value is string => typeof value === "string");
        throw new Error(messages.join(" ") || "Could not load bookings");
      }
      setItems(data);
      setHasSearched(true);
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
    <section>
      <h2 className="heading">Manager overview</h2>
      <p className="mt-2 text-slate-500">
        All passenger bookings, including cancelled history.
      </p>
      <form onSubmit={search} className="card mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <input name="flight" placeholder="Flight code" className="field" />
        <input name="passenger" placeholder="Passenger name or contact" className="field" />
        <input name="departureCountry" placeholder="From country" className="field" />
        <input name="destinationCountry" placeholder="To country" className="field" />
        <input name="departureAirport" placeholder="From airport" className="field" />
        <input name="arrivalAirport" placeholder="To airport" className="field" />
        <input name="date" type="date" className="field" />
        <select name="class" defaultValue="" className="field">
          <option value="">Any class</option>
          <option>Economy</option>
          <option>Business</option>
          <option>First</option>
        </select>
        <input name="minPrice" type="number" min="0" step="0.01" placeholder="Min booking price" className="field" />
        <input name="maxPrice" type="number" min="0" step="0.01" placeholder="Max booking price" className="field" />
        <button disabled={loading} className="rounded-lg bg-slate-900 px-4 py-3 font-semibold text-white hover:bg-slate-700 disabled:opacity-60 sm:col-span-2 lg:col-span-4">
          {loading ? "Loading bookings…" : "Apply filters"}
        </button>
      </form>
      {error && <p className="mt-4 text-red-600">{error}</p>}
      <div className="mt-6 overflow-x-auto rounded-2xl bg-white shadow-sm">
        <table className="w-full min-w-[760px] text-left text-sm">
          <thead className="bg-slate-50 text-slate-500">
            <tr>
              <th className="p-4">Flight</th>
              <th className="p-4">Passenger</th>
              <th className="p-4">Class</th>
              <th className="p-4">Price</th>
              <th className="p-4">Departure</th>
              <th className="p-4">Status</th>
            </tr>
          </thead>
          <tbody>
            {items.map((x) => (
              <tr className="border-t" key={x.booking.id}>
                <td className="p-4 font-semibold">{x.flight?.code}</td>
                <td className="p-4">{x.passenger?.name}<span className="block text-xs text-slate-400">{x.passenger?.contactDetails}</span></td>
                <td className="p-4">{x.booking.class}</td>
                <td className="p-4">{money(x.booking.finalPrice)}</td>
                <td className="p-4">{x.flight ? date(x.flight.departureAt) : "—"}</td>
                <td className="p-4"><span className={`badge ${x.booking.status === "Active" ? "bg-emerald-100 text-emerald-700" : "bg-rose-100 text-rose-700"}`}>{x.booking.status}</span></td>
              </tr>
            ))}
            {items.length === 0 && !loading && (
              <tr><td colSpan={6} className="p-10 text-center text-slate-500">{hasSearched ? "No bookings match these filters." : "No bookings found."}</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
}
