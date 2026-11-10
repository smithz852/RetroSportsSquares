import { useQuery } from "@tanstack/react-query";
import { endpoints } from "@shared/routes";
import { type PlayerStats, type CurrentGameSummary, type PaginatedPastGames } from "@shared/schema";
import { apiFetch } from "@/lib/api-client";

export function usePlayerStats() {
  return useQuery({
    queryKey: ['playerStats'],
    queryFn: async (): Promise<PlayerStats> => {
      if (!localStorage.getItem('token')) throw new Error("Not authenticated");
      const res = await apiFetch(endpoints.dashboard.stats);
      if (!res.ok) throw new Error("Failed to fetch player stats");
      return res.json();
    },
  });
}

export function useCurrentGames(enabled: boolean) {
  return useQuery({
    queryKey: ['dashboardCurrentGames'],
    queryFn: async (): Promise<CurrentGameSummary[]> => {
      if (!localStorage.getItem('token')) throw new Error("Not authenticated");
      const res = await apiFetch(endpoints.dashboard.currentGames);
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
      if (!localStorage.getItem('token')) throw new Error("Not authenticated");
      const res = await apiFetch(endpoints.dashboard.pastGames(page, pageSize));
      if (!res.ok) throw new Error("Failed to fetch past games");
      return res.json();
    },
    enabled,
  });
}
