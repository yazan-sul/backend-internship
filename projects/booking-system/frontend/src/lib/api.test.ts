import { describe, expect, test } from "bun:test";
import { apiErrorMessage } from "./api";

describe("apiErrorMessage", () => {
  test("prefers a server message", () => {
    expect(apiErrorMessage({ message: "Flight not found." }, "Fallback")).toBe(
      "Flight not found.",
    );
  });

  test("flattens validation problem errors", () => {
    expect(
      apiErrorMessage(
        { errors: { code: ["Code is required."], price: ["Price is invalid."] } },
        "Fallback",
      ),
    ).toBe("Code is required. Price is invalid.");
  });

  test("uses a safe fallback for unknown payloads", () => {
    expect(apiErrorMessage({ detail: "internal details" }, "Try again later.")).toBe(
      "Try again later.",
    );
  });
});
