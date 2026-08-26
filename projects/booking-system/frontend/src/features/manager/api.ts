import { z } from "zod";
import { apiErrorMessage } from "../../lib/api";
import { validationFieldSchema, type ValidationField } from "../../lib/flightSchema";
import { bookingRecordSchema, importResultSchema, type BookingRecord, type ImportResult } from "../../types/booking";
export async function searchManagerBookings(params = new URLSearchParams()): Promise<BookingRecord[]> { const r = await fetch(`/api/manager/bookings?${params}`); const d: unknown = await r.json(); if (!r.ok) throw new Error(apiErrorMessage(d, "Could not load bookings")); return z.array(bookingRecordSchema).parse(d); }
export async function getValidationFields(): Promise<ValidationField[]> { const r = await fetch("/api/manager/flights/validation-details"); const d: unknown = await r.json(); if (!r.ok) throw new Error("Could not load validation rules"); return z.array(validationFieldSchema).parse(d); }
export async function importFlights(file: File): Promise<ImportResult> { const body = new FormData(); body.append("file", file); const r = await fetch("/api/manager/flights/import", { method: "POST", body }); const d: unknown = await r.json(); if (!r.ok && !(typeof d === "object" && d !== null && "errors" in d)) throw new Error(apiErrorMessage(d, "Could not import flights")); return importResultSchema.parse(d); }
