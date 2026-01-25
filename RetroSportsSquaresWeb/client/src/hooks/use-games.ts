import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { type Game, type CreateGameRequest } from "@shared/schema";

export function useGames() {
  return useQuery({
    queryKey: ['games'],
    queryFn: async (): Promise<Game[]> => {
      const res = await fetch('https://localhost:7187/AvailableGames/GetAvailableGames');
      // console.log(res)
      if (!res.ok) throw new Error("Failed to fetch games");
      return res.json();
    },
  });
}

export function useCreateGame() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (data: CreateGameRequest): Promise<Game> => {
           console.log(data);
      const res = await fetch('https://localhost:7187/AvailableGames/CreateGame', {
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
