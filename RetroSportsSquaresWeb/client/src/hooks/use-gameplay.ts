import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { endpoints } from "@shared/routes";
import { apiFetch } from "@/lib/api-client";
import {
  BoardSquare,
  CreateOutsideSquareNumbersRequest,
  OutsideSquare,
  OutsideSquareNumbersResponse,
  SquareSelectionResponse,
  type CreateSquareSelectionRequest
} from "@shared/schema";

export function useJoinGame() {
  return useMutation({
    mutationFn: async (gameId: string): Promise<{ isHost: boolean }> => {
      if (!localStorage.getItem("token")) throw new Error("Please login to join a game");
      const res = await apiFetch(endpoints.games.join(gameId), { method: "POST" });
      if (!res.ok) {
        const text = await res.text().catch(() => "");
        throw new Error(text || "Failed to join game");
      }
      return res.json();
    },
  });
}

export function useLeaveGame() {
  return useMutation({
    mutationFn: async (gameId: string): Promise<void> => {
      if (!localStorage.getItem("token")) throw new Error("Please login to leave a game");
      const res = await apiFetch(endpoints.games.leave(gameId), { method: "POST" });
      if (!res.ok) {
        const text = await res.text().catch(() => "");
        throw new Error(text || "Failed to leave game");
      }
    },
  });
}

export function usePostSquareSelection(gameId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (
      data: CreateSquareSelectionRequest,
    ): Promise<SquareSelectionResponse> => {
      if (!localStorage.getItem("token")) throw new Error("Please login to create a game");
      const res = await apiFetch(endpoints.selections.create(gameId), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          selections: data.selections.map((s) => s.squareId),
        }),
      });
      
      if (!res.ok) {
        let errorMessage = "Failed to save squares";
        let errorDetails = null;
        try {
          const errorData = await res.json();
          errorMessage = errorData.message || errorData.error || errorMessage;
          // If backend returns a list of unavailable squares
          errorDetails = errorData.unavailableSquares || errorData.errors || null;
        } catch {
          errorMessage = res.statusText || errorMessage;
        }
        
        // Create custom error with details
        const error = new Error(errorMessage) as Error & { details?: any };
        error.details = errorDetails;
        throw error;
      }
      
      const selectionData = await res.json();

      // Backend returns array directly, wrap it in expected format
      return { selections: selectionData };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["createSelections"] });
      // Public-game squares are bought with coins — refresh the navbar balance
      queryClient.invalidateQueries({ queryKey: ["currentUser"] });
    },
  });
}

export function useGetBoardSquares(gameId: string, refetchInterval?: number | false) {
  return useQuery({
    queryKey: ["boardSquares", gameId],
    queryFn: async (): Promise<BoardSquare[]> => {
      const res = await apiFetch(endpoints.selections.getBoard(gameId));
      if (!res.ok) throw new Error("Failed to fetch board squares");
      return res.json();
    },
    enabled: !!gameId,
    refetchInterval: refetchInterval ?? false,
  });
}

export function useSetOutsideSquareNumbers(gameId: string) {
 const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (
      data: CreateOutsideSquareNumbersRequest,
    ): Promise<OutsideSquareNumbersResponse> => {
      if (!localStorage.getItem("token")) throw new Error("Please login to start a game");
      const res = await apiFetch(endpoints.selections.setGameNumbers(gameId), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          OutsideSquares: data.outsideSquares.map((s) => ({
            SquareName: s.squareName,
            SquareValue: s.squareValue
          })),
        }),
      });
      
      if (!res.ok) {
        let errorMessage = "Failed to save squares";
        let errorDetails = null;
        try {
          const errorData = await res.json();
          errorMessage = errorData.message || errorData.error || errorMessage;

          // If backend returns a list of unavailable squares
          errorDetails = errorData.unavailableSquares || errorData.errors || null;
        } catch {
          errorMessage = res.statusText || errorMessage;
        }

        // Create custom error with details
        const error = new Error(errorMessage) as Error & { details?: any };
        error.details = errorDetails;
        throw error;
      }
      
      const outsideSquareData = await res.json();

      // Backend returns array directly, wrap it in expected format
      return { outsideSquares: outsideSquareData };
    },
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["OutsideSquares", gameId] }),
  });
}

export function useGetOutsideSquares(gameId: string) {
  return useQuery({
    queryKey: ["OutsideSquares", gameId], 
    queryFn: async (): Promise<OutsideSquare> => {
      const res = await apiFetch(endpoints.selections.getGameNumbers(gameId));
      if (!res.ok) throw new Error("Failed to fetch outside squares");
      return res.json();
    },
    enabled: !!gameId,
  });
}