type Props = { onSearchResults: (params: URLSearchParams) => void };

export function SearchPage({ onSearchResults }: Props) {
  function search(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const params = new URLSearchParams(
      Array.from(new FormData(event.currentTarget).entries()) as [
        string,
        string,
      ][],
    );
    for (const [key, value] of params) {
      if (!value) params.delete(key);
    }
    onSearchResults(params);
  }

  return (
    <section className="mx-auto max-w-4xl py-12">
      <p className="font-semibold text-cyan-700">Your next departure</p>
      <h2 className="mt-2 text-5xl font-bold tracking-tight">
        Where will you go?
      </h2>
      <p className="mt-3 text-slate-500">
        Search live inventory and reserve the class that fits your journey.
      </p>
      <form
        onSubmit={search}
        className="mt-8 grid gap-3 rounded-2xl bg-white p-6 shadow-sm sm:grid-cols-2"
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
        <button className="rounded-lg bg-cyan-600 px-4 py-3 font-semibold text-white hover:bg-cyan-700 sm:col-span-2">
          Search flights
        </button>
      </form>
    </section>
  );
}
