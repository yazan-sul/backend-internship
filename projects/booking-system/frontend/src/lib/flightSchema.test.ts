import { describe, expect, test } from "bun:test";
import { flightSchema, validationFieldSchema } from "./flightSchema";

const validFlight = {
  id: "flight-id",
  code: "SB-100",
  departureCountry: "Palestine",
  destinationCountry: "France",
  departureAirport: "JFK",
  arrivalAirport: "CDG",
  departureAt: "2026-09-15T14:30:00Z",
  prices: { economy: 100, business: 200, first: 300 },
  availability: { economy: 10, business: 2, first: 1 },
};

describe("flight response validation", () => {
  test("accepts a valid flight response", () => {
    expect(flightSchema.safeParse(validFlight).success).toBe(true);
  });

  test("rejects malformed prices, capacities, and overlong codes", () => {
    expect(
      flightSchema.safeParse({
        ...validFlight,
        code: "THIS-CODE-IS-TOO-LONG",
        prices: { ...validFlight.prices, economy: "100" },
        availability: { ...validFlight.availability, economy: -1 },
      }).success,
    ).toBe(false);
  });

  test("rejects incomplete validation metadata", () => {
    expect(validationFieldSchema.safeParse({ field: "Code" }).success).toBe(false);
  });
});
