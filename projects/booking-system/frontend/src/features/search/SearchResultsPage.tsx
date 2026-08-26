import { useEffect, useState } from "react";
import { CountryCard } from "../../components/CountryCard";
import { writeCookie } from "../../lib/cookies";
import { date, money } from "../../lib/formatters";
import type { Flight } from "../../lib/flightSchema";
import { createBooking, createPassenger, searchFlights } from "./api";

type Props = {
  passengerName: string;
  contactDetails: string;
  passengerId: string | null;
  onPassengerCreated: (id: string) => void;
  onNewSearch: () => void;
};

export function SearchResultsPage({
  passengerName,
  contactDetails,
  passengerId,
  onPassengerCreated,
  onNewSearch,
}: Props) {
  const [flights, setFlights] = useState<Flight[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [selectedCountry, setSelectedCountry] = useState<string | null>(null);
  const params = new URLSearchParams(window.location.search);

  const countryImages: Record<string, string> = {
    France:
      "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?auto=format&fit=crop&w=900&q=80",
    Italy:
      "https://images.unsplash.com/photo-1529260830199-42c24126f198?auto=format&fit=crop&w=900&q=80",
    Spain:
      "https://images.unsplash.com/photo-1539037116277-4db20889f2d4?auto=format&fit=crop&w=900&q=80",
    "United Kingdom":
      "https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?auto=format&fit=crop&w=900&q=80",
    Greece:
      "https://images.unsplash.com/photo-1603565816030-6b389eeb23cb?auto=format&fit=crop&w=900&q=80",
    Germany:
      "https://images.unsplash.com/photo-1467269204594-9661b134dd2b?auto=format&fit=crop&w=900&q=80",
    Turkey:
      "https://images.unsplash.com/photo-1524231757912-21f4fe3a7200?auto=format&fit=crop&w=900&q=80",
    "United Arab Emirates":
      "https://images.unsplash.com/photo-1512453979798-5ea266f8880c?auto=format&fit=crop&w=900&q=80",
  };

  const getCountryImage = (country: string) =>
    countryImages[country] ??
    "https://images.unsplash.com/photo-1488646953014-85cb44e25828?auto=format&fit=crop&w=900&q=80";

  async function loadResults() {
    setLoading(true);
    setError("");
    try {
      setFlights(await searchFlights(params));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Could not load flights");
    } finally {
      setLoading(false);
    }
  }
  useEffect(() => {
    void loadResults();
  }, []);

  async function book(flight: Flight, className: string, price: number) {
    setNotice("");
    try {
      const id =
        passengerId ?? (await createPassenger(passengerName, contactDetails));
      if (!passengerId) {
        writeCookie("skybook.passengerId", id);
        onPassengerCreated(id);
      }
      await createBooking(id, flight.id, className);
      setNotice(
        `Booking confirmed: ${flight.code} · ${className} · ${money(price)} · ${date(flight.departureAt)}`,
      );
      void loadResults();
    } catch (e) {
      setNotice(e instanceof Error ? e.message : "Could not create booking");
    }
  }

  return (
    <section>
      {notice && (
        <div className="mb-6 rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-emerald-800">
          {notice}
        </div>
      )}
      <div className="mb-8 flex items-center justify-between gap-4">
        <div>
          <p className="font-semibold text-cyan-700">Search results</p>
          <h2 className="mt-1 text-3xl font-bold tracking-tight">
            {selectedCountry
              ? `Flights to ${selectedCountry}`
              : "Choose your destination"}
          </h2>
          <p className="mt-1 text-slate-500">
            {selectedCountry
              ? "Choose a cabin class to reserve your flight."
              : "Explore available flights by destination country."}
          </p>
        </div>
        <div className="flex gap-2">
          {selectedCountry && (
            <button
              onClick={() => setSelectedCountry(null)}
              className="rounded-lg border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-50"
            >
              All destinations
            </button>
          )}
          <button
            onClick={onNewSearch}
            className="rounded-lg border border-cyan-200 px-4 py-2 text-sm font-semibold text-cyan-700 hover:bg-cyan-50"
          >
            New search
          </button>
        </div>
      </div>
      {error && <p className="mb-4 text-red-600">{error}</p>}
      {loading ? (
        <div className="rounded-2xl bg-white p-10 text-center text-slate-500">
          Searching flights…
        </div>
      ) : flights.length === 0 ? (
        <div className="rounded-2xl bg-white p-10 text-center text-slate-500">
          No matching flights. Try widening your search.
        </div>
      ) : selectedCountry ? (
        <div className="grid gap-4">
          {flights
            .filter((flight) => flight.destinationCountry === selectedCountry)
            .map((flight) => (
              <FlightCard key={flight.id} flight={flight} onBook={book} />
            ))}
        </div>
      ) : (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {[...new Set(flights.map((flight) => flight.destinationCountry))].map(
            (country) => (
              <CountryCard
                key={country}
                country={country}
                flightCount={
                  flights.filter(
                    (flight) => flight.destinationCountry === country,
                  ).length
                }
                startingPrice={Math.min(
                  ...flights
                    .filter((flight) => flight.destinationCountry === country)
                    .flatMap((flight) => Object.values(flight.prices)),
                )}
                imageUrl={getCountryImage(country)}
                onExplore={() => setSelectedCountry(country)}
              />
            ),
          )}
        </div>
      )}
    </section>
  );
}

function FlightCard({
  flight,
  onBook,
}: {
  flight: Flight;
  onBook: (flight: Flight, className: string, price: number) => Promise<void>;
}) {
  const [expanded, setExpanded] = useState(false);
  const [selectedClass, setSelectedClass] = useState<
    "economy" | "business" | "first" | null
  >(null);
  const [isBooking, setIsBooking] = useState(false);
  const selectedPrice = selectedClass ? flight.prices[selectedClass] : null;
  const selectedLabel = selectedClass
    ? selectedClass[0].toUpperCase() + selectedClass.slice(1)
    : null;

  async function confirmBooking() {
    if (!selectedClass || selectedPrice === null) return;
    setIsBooking(true);
    try {
      await onBook(flight, selectedLabel ?? "Economy", selectedPrice);
    } finally {
      setIsBooking(false);
    }
  }

  return (
    <article className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm transition hover:border-slate-300 hover:shadow-md">
      <button
        type="button"
        aria-expanded={expanded}
        onClick={() => setExpanded((current) => !current)}
        className="group flex w-full flex-wrap items-start justify-between gap-5 p-5 text-left transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-cyan-200"
      >
        <div>
          <div className="flex items-center gap-3">
            <p className="text-sm font-bold text-cyan-700">{flight.code}</p>
            <span className="badge bg-slate-100 text-slate-500">
              {flight.departureCountry} → {flight.destinationCountry}
            </span>
          </div>
          <h3 className="mt-3 text-xl font-bold">
            {flight.departureAirport} <span className="text-slate-300">→</span>{" "}
            {flight.arrivalAirport}
          </h3>
          <p className="mt-1 text-sm text-slate-500">
            {date(flight.departureAt)}
          </p>
        </div>
        <span className="flex items-center gap-3 self-center text-right text-xs font-semibold uppercase tracking-wide text-slate-400">
          {expanded ? "Hide fares" : "View fares"}
          <span
            aria-hidden="true"
            className={`flex h-8 w-8 items-center justify-center rounded-full border border-slate-200 bg-white text-lg text-slate-500 shadow-sm transition-transform duration-200 group-hover:border-cyan-300 group-hover:text-cyan-600 ${expanded ? "rotate-180" : ""}`}
          >
            ↓
          </span>
        </span>
      </button>
      {expanded && (
        <>
          <div className="border-t border-slate-100 px-5 pt-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">
              Choose a fare · You’ll review before booking
            </p>
          </div>
          <div className="grid gap-3 px-5 pb-5 pt-3 sm:grid-cols-3">
        {(["economy", "business", "first"] as const).map((classKey) => (
          <button
            type="button"
            disabled={!flight.availability[classKey]}
            key={classKey}
            aria-pressed={selectedClass === classKey}
            onClick={() => setSelectedClass(classKey)}
            className={`rounded-xl border p-4 text-left transition focus:outline-none focus:ring-2 focus:ring-cyan-200 disabled:cursor-not-allowed disabled:opacity-45 ${
              selectedClass === classKey
                ? "border-cyan-500 bg-cyan-50 ring-1 ring-cyan-500"
                : "border-slate-200 hover:border-cyan-300 hover:bg-slate-50"
            }`}
          >
            <span className="flex items-center justify-between text-sm font-semibold capitalize text-slate-700">
              {classKey}
              {selectedClass === classKey && (
                <span className="text-xs font-bold text-cyan-700">
                  Selected
                </span>
              )}
            </span>
            <strong className="mt-2 block text-xl">
              {money(flight.prices[classKey])}
            </strong>
            <span className="mt-1 block text-xs text-slate-500">
              {flight.availability[classKey]
                ? `${flight.availability[classKey]} seats left`
                : "Sold out"}
            </span>
          </button>
        ))}
      </div>
      <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 bg-slate-50 px-5 py-4">
        <p className="text-sm text-slate-500">
          {selectedLabel ? (
            <>
              Selected:{" "}
              <strong className="text-slate-800">
                {selectedLabel} · {money(selectedPrice ?? 0)}
              </strong>
            </>
          ) : (
            "Choose a fare to continue"
          )}
        </p>
        <button
          type="button"
          disabled={!selectedClass || isBooking}
          onClick={() => void confirmBooking()}
          className="rounded-lg bg-cyan-600 px-5 py-2.5 text-sm font-bold text-white shadow-sm transition hover:bg-cyan-700 focus:outline-none focus:ring-2 focus:ring-cyan-200 disabled:cursor-not-allowed disabled:bg-slate-300"
        >
          {isBooking ? "Booking…" : "Book this fare"}
        </button>
      </div>
        </>
      )}
    </article>
  );
}
