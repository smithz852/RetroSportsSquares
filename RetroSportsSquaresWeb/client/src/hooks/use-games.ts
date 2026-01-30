import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import { type Game, type CreateGameRequest, type AvailableGameOptions } from "@shared/schema";

export function useGames() {
  return useQuery({
    queryKey: ['games'],
    queryFn: async (): Promise<Game[]> => {
      const res = await fetch(`${API_BASE_URL}${endpoints.games.list}`);
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
    mutationFn: async (data: CreateGameRequest): Promise<Game> => {
           console.log(data);
      const res = await fetch(`${API_BASE_URL}${endpoints.games.create}`, {
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

export function useGetAvailableGameOptions() {
  const queryClient = useQueryClient();
  return useQuery({
    queryKey: ['available-game-options'],
    queryFn: async (): Promise<AvailableGameOptions[]> => {
      const res = await fetch(`${API_BASE_URL}${endpoints.games.options}`);
      if (!res.ok) throw new Error("Failed to fetch available game options");
      return res.json();
    },
  });
}
