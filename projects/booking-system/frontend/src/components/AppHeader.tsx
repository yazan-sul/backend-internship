type Props = {
  passengerName: string;
  contactDetails: string;
  onPassengerChange: (name: string, contact: string) => void;
};
export function AppHeader({
  passengerName,
  contactDetails,
  onPassengerChange,
}: Props) {
  return (
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
            onChange={(e) => onPassengerChange(e.target.value, contactDetails)}
            placeholder="Name"
            className="ml-2 w-36 rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white"
          />
          <input
            value={contactDetails}
            onChange={(e) => onPassengerChange(passengerName, e.target.value)}
            placeholder="Contact details"
            className="ml-2 rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white"
          />
        </label>
      </div>
    </header>
  );
}
