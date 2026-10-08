import { useMutation } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import { type SupportRequest } from "@shared/schema";

async function sendSupportRequest(payload: SupportRequest): Promise<void> {
  const response = await fetch(`${API_BASE_URL}${endpoints.support.send}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const data = await response.json().catch(() => ({}));
    throw new Error(data.message ?? "Failed to send message.");
  }
}

export function useSupportRequest() {
  return useMutation({
    mutationFn: sendSupportRequest,
  });
}
