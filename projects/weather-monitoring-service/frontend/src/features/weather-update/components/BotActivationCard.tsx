type BotActivationCardProps = {
  bot: string;
  message: string;
};

function getBotAppearance(bot: string) {
  if (bot === "RainBot") {
    return { icon: "☂", className: "border-sky-400/20 bg-sky-400/10 text-sky-100" };
  }

  if (bot === "SunBot") {
    return {
      icon: "☀",
      className: "border-amber-300/20 bg-amber-300/10 text-amber-50",
    };
  }

  if (bot === "SnowBot") {
    return {
      icon: "❄",
      className: "border-cyan-200/20 bg-cyan-200/10 text-cyan-50",
    };
  }

  return { icon: "◆", className: "border-white/10 bg-white/5 text-slate-100" };
}

export function BotActivationCard({ bot, message }: BotActivationCardProps) {
  const appearance = getBotAppearance(bot);

  return (
    <li className={`flex gap-4 rounded-2xl border p-4 ${appearance.className}`}>
      <span
        aria-hidden="true"
        className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-black/15 text-xl"
      >
        {appearance.icon}
      </span>
      <div>
        <p className="text-sm font-semibold">{bot}</p>
        <p className="mt-1 text-sm leading-6 opacity-75">{message}</p>
      </div>
    </li>
  );
}
