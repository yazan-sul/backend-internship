import type { Tab } from "../types/navigation";
export function AppNav({
  tab,
  onTabChange,
}: {
  tab: Tab;
  onTabChange: (tab: Tab) => void;
}) {
  return (
    <nav className="border-b bg-white">
      <div className="mx-auto flex max-w-6xl gap-6 px-6">
        {(["search", "bookings", "manager"] as Tab[]).map((item) => (
          <button
            key={item}
            onClick={() => onTabChange(item)}
            className={`border-b-2 py-4 text-sm font-semibold capitalize ${tab === item ? "border-cyan-500 text-cyan-700" : "border-transparent text-slate-500"}`}
          >
            {item === "search" ? "Find a flight" : item}
          </button>
        ))}
      </div>
    </nav>
  );
}
