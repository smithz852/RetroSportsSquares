import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import { type ChatMessage } from "@shared/schema";

// Appends a message to the cached list, skipping duplicates by id.
// The sender receives the message twice (mutation response + hub broadcast).
export function appendChatMessage(
  old: ChatMessage[] | undefined,
  msg: ChatMessage,
): ChatMessage[] {
  if (old?.some((m) => m.id === msg.id)) return old;
  return [...(old ?? []), msg];
}

export function useChatMessages(gameId: string) {
  return useQuery({
    queryKey: ["chat", gameId],
    queryFn: async (): Promise<ChatMessage[]> => {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("Please login to view chat");
      const res = await fetch(`${API_BASE_URL}${endpoints.games.chat(gameId)}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) {
        const text = await res.text().catch(() => "");
        throw new Error(text || "Failed to load chat");
      }
      return res.json();
    },
    enabled: !!gameId,
  });
}

export function useSendChatMessage(gameId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (message: string): Promise<ChatMessage> => {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("Please login to chat");
      const res = await fetch(`${API_BASE_URL}${endpoints.games.chat(gameId)}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ message }),
      });
      if (!res.ok) {
        if (res.status === 429) {
          throw new Error("Slow down! You're sending messages too fast.");
        }
        let errorMessage = "Failed to send message";
        try {
          const text = await res.text();
          try {
            const data = JSON.parse(text);
            errorMessage = data.message || data.error || text || errorMessage;
          } catch {
            errorMessage = text || errorMessage;
          }
        } catch {
          // keep default message
        }
        throw new Error(errorMessage);
      }
      return res.json();
    },
    onSuccess: (msg) => {
      // Instant echo for the sender; the hub broadcast is deduped by id
      queryClient.setQueryData<ChatMessage[]>(["chat", gameId], (old) =>
        appendChatMessage(old, msg),
      );
    },
  });
}
