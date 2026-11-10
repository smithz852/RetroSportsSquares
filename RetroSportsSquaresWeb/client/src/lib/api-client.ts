import { API_BASE_URL } from "@shared/routes";
import { navigate } from "wouter/use-browser-location";
import { queryClient } from "./queryClient";
import { toast } from "@/hooks/use-toast";

let expiring = false;

// Shared by apiFetch (REST 401s) and use-game-hub (SignalR negotiate/reconnect auth failures)
export function handleSessionExpired() {
  if (expiring) return;
  expiring = true;
  queryClient.setQueryData(["currentUser"], null);
  toast({
    title: "Session expired",
    description: "Please log in again.",
    variant: "destructive",
  });
  navigate("/login");
}

const SAFE_METHODS = new Set(["GET", "HEAD", "OPTIONS"]);

// Double-submit CSRF token (ASP.NET Core Antiforgery's RequestToken): the server sets
// this alongside the HttpOnly auth cookie specifically so JS can read it and echo it
// back as a header — an attacker's page can't read it (it belongs to the API's own
// origin). __Host- prefixed so the browser itself enforces Secure/no-Domain/Path=/.
export function getCsrfToken(): string | null {
  const match = document.cookie.match(/(?:^|; )__Host-XSRF-TOKEN=([^;]*)/);
  return match ? decodeURIComponent(match[1]) : null;
}

export async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const method = (init.method ?? "GET").toUpperCase();
  const headers = new Headers(init.headers);

  if (!SAFE_METHODS.has(method)) {
    const csrfToken = getCsrfToken();
    if (csrfToken) headers.set("X-XSRF-TOKEN", csrfToken);
  }

  const res = await fetch(`${API_BASE_URL}${path}`, { ...init, headers, credentials: "include" });

  // The auth cookie is HttpOnly, so this is the only authenticated-endpoint call
  // path left (fetchUser bypasses apiFetch to allow a silent, no-toast 401 for
  // visitors who were never logged in — see use-auth.ts).
  if (res.status === 401) handleSessionExpired();

  return res;
}
