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
        try {
          const errorData = await res.json();
          errorMessage = errorData.message || errorData.error || errorMessage;
          // console.log("Error from backend:", errorMessage);
          //******* add check for refetching data for the unavialble square once the fetch is made ********
        } catch {
          // If JSON parsing fails, use status text or default
          errorMessage = res.statusText || errorMessage;
        }
        throw new Error(errorMessage);
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
