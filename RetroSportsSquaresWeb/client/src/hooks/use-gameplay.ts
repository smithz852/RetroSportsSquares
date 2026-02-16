import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import {
  SquareSelectionResponse,
  type CreateSquareSelectionRequest,
} from "@shared/schema";

export function usePostSquareSelection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (
      data: CreateSquareSelectionRequest,
    ): Promise<SquareSelectionResponse> => {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("Please login to create a game");
      console.log("Squaredata:", data);
      const res = await fetch(`${API_BASE_URL}${endpoints.selections.create}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          selections: data.selections.map((s) => s.squareName),
        }),
      });
      if (!res.ok) throw new Error("Failed to save squares");
      console.log(res.json())
      return res.json();
    },
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["createSelections"] }),
  });
}
