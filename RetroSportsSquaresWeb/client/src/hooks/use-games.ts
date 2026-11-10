import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { endpoints } from "@shared/routes";
import { type SquareGame, type CreateSquareGameRequest, type AvailableGameOptions, type AvailableSportLeague, SquareGameScoreData, TurnStatus } from "@shared/schema";
import { apiFetch } from "@/lib/api-client";

export function useGames() {
  return useQuery({
    queryKey: ['games'],
    queryFn: async (): Promise<SquareGame[]> => {
      const res = await apiFetch(endpoints.games.list);
      if (!res.ok) throw new Error("Failed to fetch games");
      const data = await res.json();
        // console.log(data);
      return data;
    },
  });
}

export function useCreateGame() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateSquareGameRequest): Promise<SquareGame> => {
      if (!localStorage.getItem('token')) throw new Error("Please login to create a game");

      console.log(data);
      const res = await apiFetch(endpoints.games.create, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });
      if (!res.ok) throw new Error("Failed to create game");
      return res.json();
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['games'] }),
  });
}

export function useAvailableSportsAndLeagues() {
  return useQuery({
    queryKey: ['availableSportsAndLeagues'],
    queryFn: async (): Promise<AvailableSportLeague[]> => {
      const res = await apiFetch(endpoints.games.availableSportsAndLeagues);
      if (!res.ok) throw new Error('Failed to fetch available sports and leagues');
      return res.json();
    },
  });
}

export function useGetAvailableGameOptions(gameType: string, leagueId: string) {
  return useQuery({
    queryKey: ['available-game-options', gameType, leagueId],
    queryFn: async (): Promise<AvailableGameOptions[]> => {
      const res = await apiFetch(endpoints.games.options(gameType, leagueId));
      if (!res.ok) throw new Error("Failed to fetch available game options");
     const data = await res.json();
     console.log(data);
     return data;
    },
    enabled: !!gameType && !!leagueId,
  });
}

export function GetGameScoreData(id: string, refetchInterval?: number | false) {
  return useQuery({
    queryKey: ['gameScoreData', id],
    queryFn: async (): Promise<SquareGameScoreData> => {
      const res = await apiFetch(endpoints.games.scoreData(id));
      if (!res.ok) throw new Error("Failed to fetch game score data");
       const data = await res.json();
     console.log(data);
     return data;
    },
    enabled: !!id,
    refetchInterval,
  });
}

export function useStartGame(gameId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (): Promise<void> => {
      if (!localStorage.getItem('token')) throw new Error('Please login to start a game');
      const res = await apiFetch(endpoints.games.start(gameId), { method: 'POST' });
      if (!res.ok) throw new Error('Failed to start game');
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['OutsideSquares', gameId] }),
  });
}

export function useGetTurnStatus(gameId: string, enabled: boolean) {
  return useQuery({
    queryKey: ['turnStatus', gameId],
    queryFn: async (): Promise<TurnStatus> => {
      const res = await apiFetch(endpoints.games.turnStatus(gameId));
      if (!res.ok) throw new Error('Failed to fetch turn status');
      return res.json();
    },
    enabled: !!gameId && enabled,
    refetchInterval: enabled ? 30000 : false,
  });
}

export function useBeginSelections(gameId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (): Promise<TurnStatus> => {
      if (!localStorage.getItem('token')) throw new Error('Not authenticated');
      const res = await apiFetch(endpoints.games.beginSelections(gameId), { method: 'POST' });
      if (!res.ok) {
        const text = await res.text().catch(() => '');
        throw new Error(text || 'Failed to begin selections');
      }
      return res.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['turnStatus', gameId] });
      queryClient.invalidateQueries({ queryKey: ['game', gameId] });
    },
  });
}

export function useSkipPlayer(gameId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    // expectedUserId = the player the host is skipping; the server ignores the skip if the turn already moved
    mutationFn: async (expectedUserId: string): Promise<TurnStatus> => {
      if (!localStorage.getItem('token')) throw new Error('Not authenticated');
      const res = await apiFetch(`${endpoints.games.skipPlayer(gameId)}?expectedUserId=${encodeURIComponent(expectedUserId)}`, {
        method: 'POST',
      });
      if (!res.ok) throw new Error('Failed to skip player');
      return res.json();
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['turnStatus', gameId] }),
  });
}

export function useDeleteGame() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (gameId: string): Promise<void> => {
      if (!localStorage.getItem('token')) throw new Error('Please login to delete a game');
      const res = await apiFetch(endpoints.games.delete(gameId), { method: 'DELETE' });
      if (!res.ok) {
        const text = await res.text().catch(() => '');
        throw new Error(text || 'Failed to delete game');
      }
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['games'] }),
  });
}

export function getSquareGameById(id: string) {
  return useQuery({
    queryKey: ['game', id],
    queryFn: async (): Promise<SquareGame> => {
      const res = await apiFetch(endpoints.games.squareGameById(id));
      if (!res.ok) throw new Error('Failed to fetch game');
      return res.json();
    },
    enabled: !!id,
  });
}
