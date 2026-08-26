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

export function App() {
  const [tab, setTab] = useState<Tab>("search");
  const [flights, setFlights] = useState<Flight[]>([]);
  const [error, setError] = useState("");
  const [email, setEmail] = useState("demo@skybook.test");
  const [notice, setNotice] = useState("");
  const search = async (event?: React.FormEvent) => {
    event?.preventDefault();
    setError("");
    try {
      const form = event?.currentTarget as HTMLFormElement | undefined;
      const params = form
        ? new URLSearchParams(
            Array.from(new FormData(form).entries()) as [string, string][],
          )
        : new URLSearchParams();
      const response = await fetch(`/api/flights?${params}`);
      if (!response.ok) throw new Error("Could not load flights");
      setFlights(z.array(flightSchema).parse(await response.json()));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Unexpected error");
    }
  };
  useEffect(() => {
    void search();
  }, []);
  async function book(flight: Flight, cls: string, price: number) {
    const response = await fetch("/api/bookings", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        flightId: flight.id,
        class: cls,
        name: "Demo Passenger",
        email,
        contactDetails: "Email",
      }),
    });
    const data = await response.json();
    setNotice(
      response.ok
        ? `Booking confirmed: ${data.booking.id} · ${money(price)}`
        : data.message,
    );
    if (response.ok) void search();
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
            Passenger email{" "}
            <input
              value={email}
              onChange={(e) => setEmail(e.target.value)}
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
              <button className="rounded-lg bg-cyan-600 px-4 py-3 font-semibold text-white hover:bg-cyan-700 sm:col-span-5">
                Search flights
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
        {tab === "bookings" && <Bookings email={email} />}
        {tab === "manager" && <Manager />}
      </main>
    </div>
  );
}
function Bookings({ email }: { email: string }) {
  const [items, setItems] = useState<any[]>([]);
  useEffect(() => {
    fetch(`/api/bookings/me?email=${encodeURIComponent(email)}`)
      .then((r) => r.json())
      .then(setItems);
  }, [email]);
  return (
    <section>
      <h2 className="heading">My bookings</h2>
      <div className="mt-6 grid gap-3">
        {items.length === 0 ? (
          <div className="card text-slate-500">
            No bookings for this passenger yet.
          </div>
        ) : (
          items.map((x) => (
            <div
              className="card flex items-center justify-between"
              key={x.booking.id}
            >
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
          ))
        )}
      </div>
    </section>
  );
}
function Manager() {
  const [items, setItems] = useState<any[]>([]);
  useEffect(() => {
    fetch("/api/manager/bookings")
      .then((r) => r.json())
      .then(setItems);
  }, []);
  return (
    <section>
      <h2 className="heading">Manager overview</h2>
      <p className="mt-2 text-slate-500">
        All passenger bookings, including cancelled history.
      </p>
      <div className="mt-6 overflow-hidden rounded-2xl bg-white shadow-sm">
        <table className="w-full text-left text-sm">
          <thead className="bg-slate-50 text-slate-500">
            <tr>
              <th className="p-4">Flight</th>
              <th className="p-4">Passenger</th>
              <th className="p-4">Class</th>
              <th className="p-4">Status</th>
            </tr>
          </thead>
          <tbody>
            {items.map((x) => (
              <tr className="border-t" key={x.booking.id}>
                <td className="p-4 font-semibold">{x.flight?.code}</td>
                <td className="p-4">{x.passenger?.email}</td>
                <td className="p-4">{x.booking.class}</td>
                <td className="p-4">{x.booking.status}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
