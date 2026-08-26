import { useEffect, useState } from "react";
import { AppHeader } from "./components/AppHeader";
import { AppNav } from "./components/AppNav";
import { BookingsPage } from "./features/bookings/BookingsPage";
import { ManagerPage } from "./features/manager/ManagerPage";
import { SearchPage } from "./features/search/SearchPage";
import { SearchResultsPage } from "./features/search/SearchResultsPage";
import { usePassengerSession } from "./hooks/usePassengerSession";
import type { Tab } from "./types/navigation";

export function App() {
  const [tab, setTab] = useState<Tab>("search");
  const [path, setPath] = useState(() => window.location.pathname);
  const passenger = usePassengerSession();
  useEffect(() => {
    const handlePopState = () => setPath(window.location.pathname);
    window.addEventListener("popstate", handlePopState);
    return () => window.removeEventListener("popstate", handlePopState);
  }, []);
  function openSearchResults(params: URLSearchParams) {
    window.history.pushState({}, "", `/search-results?${params}`);
    setPath("/search-results");
  }
  function openSearch() {
    window.history.pushState({}, "", "/");
    setPath("/");
  }
  return <div className="min-h-screen bg-[#f5f7fb] text-slate-900">
    <AppHeader passengerName={passenger.name} contactDetails={passenger.contactDetails} onPassengerChange={passenger.updateDetails} />
    <AppNav tab={tab} onTabChange={setTab} />
    <main className="mx-auto max-w-6xl px-6 py-10">
      {tab === "search" && path === "/" && <SearchPage onSearchResults={openSearchResults} />}
      {tab === "search" && path === "/search-results" && <SearchResultsPage passengerName={passenger.name} contactDetails={passenger.contactDetails} passengerId={passenger.id} onPassengerCreated={passenger.setId} onNewSearch={openSearch} />}
      {tab === "bookings" && <BookingsPage passengerId={passenger.id} />}
      {tab === "manager" && <ManagerPage />}
    </main>
  </div>;
}
