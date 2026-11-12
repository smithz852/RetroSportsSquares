import { queryClient } from "./queryClient";
import type { User } from "@shared/schema";

// No ad/analytics vendor exists in this app yet (see architecture.md) — this module
// has no caller today. It exists so the day a vendor SDK is added, its init call
// becomes `if (canTrackForAdsOrAnalytics()) initVendorSdk()` and CCPA/CPRA opt-out
// (including Global Privacy Control) is already honored, with no new plumbing.
//
// Follow-ups once a vendor actually lands (tracked in project_launch_readiness.md):
// - Add a public "Do Not Sell or Share My Personal Information" link (Footer +
//   Landing) that's reachable without login, writing ANONYMOUS_OPT_OUT_KEY below.
// - Wrap the vendor's SDK init with this gate.
// - If the vendor supports consent-signal forwarding (e.g. Google Restricted Data
//   Processing, Meta LDU), wire the stored preference through to it too.

const ANONYMOUS_OPT_OUT_KEY = "dnsmpi_opt_out";

function hasGlobalPrivacyControlSignal(): boolean {
  try {
    return (navigator as Navigator & { globalPrivacyControl?: boolean }).globalPrivacyControl === true;
  } catch {
    return false;
  }
}

function anonymousVisitorOptedOut(): boolean {
  try {
    return localStorage.getItem(ANONYMOUS_OPT_OUT_KEY) === "true";
  } catch {
    return false;
  }
}

function loggedInUserOptedOut(): boolean {
  const user = queryClient.getQueryData<User | null>(["currentUser"]);
  return user?.doNotSellOrShare === true;
}

// Returns false if the visitor (logged in or anonymous) has opted out of the sale/
// sharing of personal information, via Global Privacy Control, their account
// preference, or a prior anonymous opt-out. Call this before initializing any
// ad or analytics SDK that would share data with a third party.
export function canTrackForAdsOrAnalytics(): boolean {
  if (hasGlobalPrivacyControlSignal()) return false;
  if (anonymousVisitorOptedOut()) return false;
  if (loggedInUserOptedOut()) return false;
  return true;
}
