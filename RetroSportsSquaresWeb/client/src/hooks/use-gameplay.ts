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
      console.log("res status:", res.status, "ok:", res.ok);
      if (!res.ok) throw new Error("Failed to save squares");
      
      const selectionData = await res.json();
      console.log("selectionResponse:", selectionData);
      
      // Backend returns array directly, wrap it in expected format
      return { selections: selectionData };
    },
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["createSelections"] }),
  });
}
