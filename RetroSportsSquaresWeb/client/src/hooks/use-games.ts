import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import { type SquareGame, type CreateSquareGameRequest, type AvailableGameOptions, SquareGameScoreData } from "@shared/schema";

export function useGames() {
  return useQuery({
    queryKey: ['games'],
    queryFn: async (): Promise<SquareGame[]> => {
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
    mutationFn: async (data: CreateSquareGameRequest): Promise<SquareGame> => {
      const token = localStorage.getItem('token');
      // console.log(token);
      if (!token) throw new Error("Please login to create a game");
      
      console.log(data);
      const res = await fetch(`${API_BASE_URL}${endpoints.games.create}`, {
        method: "POST",
        headers: { 
          "Content-Type": "application/json",
          'Authorization': `Bearer ${token}`
         },
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
     const data = await res.json();
     console.log(data);
     return data;
    },
  });
}

export function GetGameScoreData() {
  const queryClient = useQueryClient();
  return useQuery({
    queryKey: ['gameScoreData'],
    queryFn: async (): Promise<SquareGameScoreData[]> => {
      const res = await fetch(`${API_BASE_URL}${endpoints.games.scoreData}`);
      if (!res.ok) throw new Error("Failed to fetch available game options");
     const data = await res.json();
     console.log(data);
     return data;
    },
  });
}
