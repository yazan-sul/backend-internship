import { useEffect, useState } from "react";
import { money } from "../../../lib/formatters";
import type { BookingRecord } from "../../../types/booking";
import type { ManagerView } from "../ManagerPage";
import { searchManagerBookings } from "../api";
import { ManagerPageIntro } from "../components/ManagerPageIntro";
import { ManagerStatCard } from "../components/ManagerStatCard";

export function ManagerOverviewPage({
  onNavigate,
}: {
  onNavigate: (view: ManagerView) => void;
}) {
  const [items, setItems] = useState<BookingRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  useEffect(() => {
    searchManagerBookings()
      .then(setItems)
      .catch((e) =>
        setError(e instanceof Error ? e.message : "Could not load overview"),
      )
      .finally(() => setLoading(false));
  }, []);
  const active = items.filter(
    (item) => item.booking.status === "Active",
  ).length;
  const revenue = items.reduce((sum, item) => sum + item.booking.finalPrice, 0);
  return (
    <div className="space-y-6">
      <ManagerPageIntro
        eyebrow="Today"
        title="A clear view of your operations"
        description="Start with the numbers, then jump directly to the task you need."
      />
      {error && <p className="text-sm text-red-600">{error}</p>}
      <div className="grid gap-4 sm:grid-cols-3">
        <ManagerStatCard
          label="Bookings"
          value={loading ? "—" : items.length}
          detail="All booking records"
        />
        <ManagerStatCard
          label="Active"
          value={loading ? "—" : active}
          detail="Current reservations"
        />
        <ManagerStatCard
          label="Revenue"
          value={loading ? "—" : money(revenue)}
          detail="From shown bookings"
        />
      </div>
      <div className="grid gap-4 md:grid-cols-3">
        <QuickAction
          title="Review bookings"
          text="Search passenger bookings and statuses."
          onClick={() => onNavigate("bookings")}
        />
        <QuickAction
          title="Import flights"
          text="Add new flight inventory from CSV."
          onClick={() => onNavigate("import")}
        />
        <QuickAction
          title="Check validation"
          text="See the rules used for flight imports."
          onClick={() => onNavigate("validation")}
        />
      </div>
    </div>
  );
}

function QuickAction({
  title,
  text,
  onClick,
}: {
  title: string;
  text: string;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      className="group rounded-2xl border border-slate-200 bg-white p-5 text-left shadow-sm transition hover:-translate-y-0.5 hover:border-cyan-200 hover:shadow-md"
    >
      <span className="flex items-center justify-between font-bold text-slate-900">
        {title}
        <span className="text-lg text-cyan-600 transition group-hover:translate-x-1">
          →
        </span>
      </span>
      <span className="mt-2 block text-sm leading-6 text-slate-500">
        {text}
      </span>
    </button>
  );
}
