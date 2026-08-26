import { useState } from "react";
import { removeCookie, readCookie } from "../lib/cookies";
const passengerCookie = "skybook.passengerId";
export function usePassengerSession() {
  const [name, setName] = useState("Demo Passenger");
  const [contactDetails, setContactDetails] = useState("demo@skybook.test");
  const [id, setId] = useState<string | null>(() =>
    readCookie(passengerCookie),
  );
  function updateDetails(nextName: string, nextContactDetails: string) {
    removeCookie(passengerCookie);
    setId(null);
    setName(nextName);
    setContactDetails(nextContactDetails);
  }
  return { name, contactDetails, id, setId, updateDetails };
}
