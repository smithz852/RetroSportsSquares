import { useQuery } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import { type PlayerStats, type CurrentGameSummary, type PaginatedPastGames } from "@shared/schema";

function getToken() {
  return localStorage.getItem('token');
}

export function usePlayerStats() {
  return useQuery({
    queryKey: ['playerStats'],
    queryFn: async (): Promise<PlayerStats> => {
      const token = getToken();
      if (!token) throw new Error("Not authenticated");
      const res = await fetch(`${API_BASE_URL}${endpoints.dashboard.stats}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch player stats");
      return res.json();
    },
  });
}

export function useCurrentGames(enabled: boolean) {
  return useQuery({
    queryKey: ['dashboardCurrentGames'],
    queryFn: async (): Promise<CurrentGameSummary[]> => {
      const token = getToken();
      if (!token) throw new Error("Not authenticated");
      const res = await fetch(`${API_BASE_URL}${endpoints.dashboard.currentGames}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch current games");
      return res.json();
    },
    enabled,
  });
}

export function usePastGames(page: number, pageSize: number, enabled: boolean) {
  return useQuery({
    queryKey: ['dashboardPastGames', page, pageSize],
    queryFn: async (): Promise<PaginatedPastGames> => {
      const token = getToken();
      if (!token) throw new Error("Not authenticated");
      const res = await fetch(`${API_BASE_URL}${endpoints.dashboard.pastGames(page, pageSize)}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch past games");
      return res.json();
    },
    enabled,
  });
}
