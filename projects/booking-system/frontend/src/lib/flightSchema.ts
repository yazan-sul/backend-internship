import { z } from "zod";

export const flightSchema = z.object({
  id: z.string(),
  code: z.string().min(1).max(12),
  departureCountry: z.string().min(1).max(80),
  destinationCountry: z.string().min(1).max(80),
  departureAirport: z.string().min(1).max(12),
  arrivalAirport: z.string().min(1).max(12),
  departureAt: z.string(),
  prices: z.object({
    economy: z.number().positive(),
    business: z.number().positive(),
    first: z.number().positive(),
  }),
  availability: z.object({
    economy: z.number().int().nonnegative(),
    business: z.number().int().nonnegative(),
    first: z.number().int().nonnegative(),
  }),
});

export const validationFieldSchema = z.object({
  field: z.string(),
  displayName: z.string(),
  type: z.string(),
  required: z.boolean(),
  min: z.union([z.number(), z.string()]).nullable(),
  max: z.union([z.number(), z.string()]).nullable(),
  minLength: z.number().nullable(),
  maxLength: z.number().nullable(),
  options: z.array(z.string()),
  customRules: z.array(z.string()),
});

export type Flight = z.infer<typeof flightSchema>;
export type ValidationField = z.infer<typeof validationFieldSchema>;
