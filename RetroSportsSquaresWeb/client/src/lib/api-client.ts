import { API_BASE_URL } from "@shared/routes";
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
