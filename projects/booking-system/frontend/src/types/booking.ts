import { z } from "zod";

export const bookingSchema = z.object({
  id: z.string(),
  flightId: z.string(),
  class: z.string(),
  finalPrice: z.number(),
  status: z.string(),
});
export const bookingRecordSchema = z.object({
  booking: bookingSchema,
  flight: z
    .object({
      code: z.string(),
      departureAirport: z.string(),
      arrivalAirport: z.string(),
      departureAt: z.string(),
    })
    .nullable()
    .optional(),
  passenger: z
    .object({ name: z.string(), contactDetails: z.string() })
    .nullable()
    .optional(),
});
export const importResultSchema = z.object({
  imported: z.number().optional(),
  errors: z
    .array(
      z.object({
        row: z.union([z.number(), z.string()]),
        field: z.string(),
        message: z.string(),
        value: z.union([z.string(), z.number()]).optional(),
      }),
    )
    .optional(),
});
export type BookingRecord = z.infer<typeof bookingRecordSchema>;
export type ImportResult = z.infer<typeof importResultSchema>;
