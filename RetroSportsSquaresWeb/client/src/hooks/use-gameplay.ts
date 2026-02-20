import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import {
  SquareSelectionResponse,
  type CreateSquareSelectionRequest,
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
