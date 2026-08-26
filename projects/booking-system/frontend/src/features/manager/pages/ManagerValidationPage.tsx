import { useEffect, useState } from "react";
import type { ValidationField } from "../../../lib/flightSchema";
import { getValidationFields } from "../api";
import { ManagerPageIntro } from "../components/ManagerPageIntro";

export function ManagerValidationPage() {
  const [fields, setFields] = useState<ValidationField[]>([]);
  const [error, setError] = useState("");
  useEffect(() => {
    getValidationFields()
      .then(setFields)
      .catch((e) =>
        setError(
          e instanceof Error ? e.message : "Could not load validation rules",
        ),
      );
  }, []);
  return (
    <div className="space-y-6">
      <ManagerPageIntro
        eyebrow="Flight inventory"
        title="Validation rules"
        description="These rules are generated from the backend Flight model and applied to every import."
      />
      {error && <p className="text-sm text-red-600">{error}</p>}
      <div className="grid gap-4 md:grid-cols-2">
        {fields.map((field) => (
          <article
            key={field.field}
            className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"
          >
            <div className="flex items-start justify-between gap-3">
              <div>
                <h4 className="font-semibold text-slate-900">
                  {field.displayName}
                </h4>
                <p className="mt-1 text-xs text-slate-500">
                  {field.field} · {field.type}
                </p>
              </div>
              {field.required && (
                <span className="badge bg-amber-100 text-amber-800">
                  Required
                </span>
              )}
            </div>
            <div className="mt-4 flex flex-wrap gap-2 text-xs text-slate-600">
              {field.min !== null && (
                <span className="rounded-lg bg-slate-50 px-2 py-1">
                  Min: {field.min}
                </span>
              )}
              {field.max !== null && (
                <span className="rounded-lg bg-slate-50 px-2 py-1">
                  Max: {field.max}
                </span>
              )}
              {field.minLength !== null && (
                <span className="rounded-lg bg-slate-50 px-2 py-1">
                  Min length: {field.minLength}
                </span>
              )}
              {field.maxLength !== null && (
                <span className="rounded-lg bg-slate-50 px-2 py-1">
                  Max length: {field.maxLength}
                </span>
              )}
            </div>
            {field.options.length > 0 && (
              <p className="mt-3 text-xs text-slate-600">
                <strong>Options:</strong> {field.options.join(", ")}
              </p>
            )}
            {field.customRules.map((rule) => (
              <p className="mt-1 text-xs text-slate-600" key={rule}>
                <strong>Rule:</strong> {rule}
              </p>
            ))}
          </article>
        ))}
      </div>
    </div>
  );
}
