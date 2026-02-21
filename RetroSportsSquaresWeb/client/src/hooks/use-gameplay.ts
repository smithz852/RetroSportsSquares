import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import {
  CreateOutsideSquareNumbersRequest,
  OutsdieSquareNumbersResponse,
  SquareSelectionResponse,
  type CreateSquareSelectionRequest, type SelectedSquares
} from "@shared/schema";

export function usePostSquareSelection(gameId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (
      data: CreateSquareSelectionRequest,
    ): Promise<SquareSelectionResponse> => {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("Please login to create a game");
      console.log("Squaredata:", data);
      const res = await fetch(`${API_BASE_URL}${endpoints.selections.create(gameId)}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          selections: data.selections.map((s) => s.squareName),
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
      // console.log("selectionResponse:", selectionData);
      
      // Backend returns array directly, wrap it in expected format
      return { selections: selectionData };
    },
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["createSelections"] }),
  });
}

export function useGetSelectedSquares(gameId: string) {
   return useQuery({
    queryKey: ["gameSelections", gameId],
    queryFn: async (): Promise<SelectedSquares[]> => {
      const res = await fetch(`${API_BASE_URL}${endpoints.selections.get(gameId)}`);
      if (!res.ok) throw new Error("Failed to fetch selections");
      const data = await res.json();
      return data;
    },
    enabled: !!gameId,
  });
}

export function useSetOutsideSquareNumbers(gameId: string) {
 const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (
      data: CreateOutsideSquareNumbersRequest,
    ): Promise<OutsdieSquareNumbersResponse> => {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("Please login to start a game");
      // console.log("OutsideSquareData:", data);
      const res = await fetch(`${API_BASE_URL}${endpoints.selections.setGameNumbers(gameId)}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          outsideSquares: data.outsideSquares.map((s) => ({
            squareName: s.squareName,
            squareValue: s.squareValue
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
      // console.log("selectionResponse:", selectionData);
      
      // Backend returns array directly, wrap it in expected format
      return { outsideSquares: outsideSquareData };
    },
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["setOutsideSquares"] }),
  });
}


// TODO: Add this when you need to fetch outside squares
// export function useGetOutsideSquares(gameId: string) {
//   return useQuery({
//     queryKey: ["setOutsideSquares", gameId], // ← Must match invalidation key
//     queryFn: async () => {
//       const res = await fetch(`${API_BASE_URL}${endpoints.selections.getGameNumbers(gameId)}`);
//       if (!res.ok) throw new Error("Failed to fetch outside squares");
//       return res.json();
//     },
//     enabled: !!gameId,
//   });
// }
