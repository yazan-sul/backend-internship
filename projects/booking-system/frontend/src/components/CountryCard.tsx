import { money } from "../lib/formatters";

type CountryCardProps = {
  country: string;
  flightCount: number;
  startingPrice: number;
  imageUrl: string;
  onExplore: () => void;
};

export function CountryCard({
  country,
  flightCount,
  startingPrice,
  imageUrl,
  onExplore,
}: CountryCardProps) {
  return (
    <article
      onClick={onExplore}
      className="group cursor-pointer overflow-hidden rounded-2xl bg-white shadow-md transition hover:-translate-y-1 hover:shadow-xl"
    >
      <div
        className="relative h-52 bg-cover bg-center"
        style={{ backgroundImage: `url(${imageUrl})` }}
      >
        <div className="absolute inset-0 bg-gradient-to-t from-slate-950/75 via-slate-950/10 to-transparent" />
        <p className="absolute bottom-4 left-5 text-sm font-medium text-white/80">
          {flightCount} {flightCount === 1 ? "flight" : "flights"} available
        </p>
      </div>
      <div className="p-5">
        <h3 className="text-xl font-bold text-slate-900">{country}</h3>
        <p className="mt-1 text-sm text-slate-500">
          Flights from{" "}
          <span className="font-semibold text-slate-700">
            {money(startingPrice)}
          </span>
        </p>
        <button
          onClick={(event) => {
            event.stopPropagation();
            onExplore();
          }}
          className="mt-5 flex w-full items-center justify-between border-t border-slate-100 pt-4 text-sm font-semibold text-slate-400 transition group-hover:text-cyan-600"
        >
          <span>Explore flights</span>
          <span
            aria-hidden="true"
            className="text-lg transition-transform group-hover:translate-x-1"
          >
            →
          </span>
        </button>
      </div>
    </article>
  );
}
