import { useQuery, useQueryClient } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import {
  type AdminSummary,
  type AdminCurrentGame,
  type AdminPaginatedPastGames,
  type AdminPlayerStats,
  type AdminUserSummary,
} from "@shared/schema";

// Admin data has no SignalR invalidation, so use a short staleTime
// instead of the app-wide infinite default
const ADMIN_STALE_TIME = 1000 * 30;

async function adminFetch<T>(endpoint: string): Promise<T> {
  const token = localStorage.getItem('token');
  if (!token) throw new Error("Not authenticated");
  const res = await fetch(`${API_BASE_URL}${endpoint}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (res.status === 403) throw new Error("Access denied: admin only");
  if (!res.ok) throw new Error("Failed to fetch admin data");
  return res.json();
}

export function useAdminSummary() {
  return useQuery({
    queryKey: ['adminSummary'],
    queryFn: () => adminFetch<AdminSummary>(endpoints.admin.summary),
    staleTime: ADMIN_STALE_TIME,
  });
}

export function useAdminCurrentGames() {
  return useQuery({
    queryKey: ['adminCurrentGames'],
    queryFn: () => adminFetch<AdminCurrentGame[]>(endpoints.admin.currentGames),
    staleTime: ADMIN_STALE_TIME,
  });
}

export function useAdminPastGames(page: number, pageSize: number) {
  return useQuery({
    queryKey: ['adminPastGames', page, pageSize],
    queryFn: () => adminFetch<AdminPaginatedPastGames>(endpoints.admin.pastGames(page, pageSize)),
    staleTime: ADMIN_STALE_TIME,
  });
}

export function useAdminPlayerStats() {
  return useQuery({
    queryKey: ['adminPlayerStats'],
    queryFn: () => adminFetch<AdminPlayerStats[]>(endpoints.admin.playerStats),
    staleTime: ADMIN_STALE_TIME,
  });
}

export function useAdminUsers() {
  return useQuery({
    queryKey: ['adminUsers'],
    queryFn: () => adminFetch<AdminUserSummary[]>(endpoints.admin.users),
    staleTime: ADMIN_STALE_TIME,
  });
}

export function useRefreshAdminData() {
  const queryClient = useQueryClient();
  return () => {
    queryClient.invalidateQueries({ queryKey: ['adminSummary'] });
    queryClient.invalidateQueries({ queryKey: ['adminCurrentGames'] });
    queryClient.invalidateQueries({ queryKey: ['adminPastGames'] });
    queryClient.invalidateQueries({ queryKey: ['adminPlayerStats'] });
    queryClient.invalidateQueries({ queryKey: ['adminUsers'] });
  };
}
