export function ManagerPageIntro({
  eyebrow,
  title,
  description,
}: {
  eyebrow: string;
  title: string;
  description: string;
}) {
  return (
    <div>
      <p className="text-sm font-semibold text-cyan-700">{eyebrow}</p>
      <h3 className="mt-1 text-2xl font-bold tracking-tight text-slate-950">
        {title}
      </h3>
      <p className="mt-2 text-sm text-slate-500">{description}</p>
    </div>
  );
}
