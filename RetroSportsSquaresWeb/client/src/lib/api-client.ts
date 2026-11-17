import { API_BASE_URL } from "@shared/routes";
import { type ApiErrorResponse } from "@shared/schema";
import { navigate } from "wouter/use-browser-location";
import { queryClient } from "./queryClient";
import { toast } from "@/hooks/use-toast";

let expiring = false;

// Shared by apiFetch (REST 401s) and use-game-hub (SignalR negotiate/reconnect auth failures)
export function handleSessionExpired() {
  if (expiring) return;
  expiring = true;
  localStorage.removeItem("token");
  queryClient.setQueryData(["currentUser"], null);
  toast({
    title: "Session expired",
    description: "Please log in again.",
    variant: "destructive",
  });
  navigate("/login");
}

export async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const token = localStorage.getItem("token");
  const headers = new Headers(init.headers);
  if (token) headers.set("Authorization", `Bearer ${token}`);

  const res = await fetch(`${API_BASE_URL}${path}`, { ...init, headers });

  // Only a real session expiry: a request made without a token (e.g. bad-password
  // login) also gets a 401, but shouldn't trigger the global expired-session flow.
  if (res.status === 401 && token) handleSessionExpired();

  return res;
}

// Every endpoint now responds to a failure with the same ApiErrorResponse shape
// ({message, fieldErrors?}) — parse it once here instead of each hook re-implementing
// its own text/json/fallback logic. `fallback` covers responses with no body at all
// (e.g. a bare 401/403 from an [Authorize] attribute).
export async function parseApiError(res: Response, fallback: string): Promise<Error> {
  const body = await res.json().catch(() => null) as ApiErrorResponse | null;
  return new Error(body?.message || fallback);
}
