import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import { type LoginRequest, type User, type LoginResponse, type SignupRequest } from "@shared/schema";
import { apiFetch, getCsrfToken } from "@/lib/api-client";



async function fetchUser(): Promise<User | null> {
  // Bypasses apiFetch deliberately: the auth cookie is HttpOnly so there's no way to
  // check client-side whether a session exists, and this runs on every page load
  // (including anonymous ones). A 401 here just means "not logged in" — it must stay
  // silent, not trigger apiFetch's session-expired toast + redirect.
  const response = await fetch(`${API_BASE_URL}${endpoints.auth.getUser}`, { credentials: "include" });
  if (!response.ok) return null;
  return response.json();
}

async function logout(): Promise<void> {
  try {
    // Rotates the security stamp (invalidating this user's tokens on all devices)
    // and clears the auth cookies server-side.
    const csrfToken = getCsrfToken();
    await fetch(`${API_BASE_URL}${endpoints.auth.logout}`, {
      method: 'POST',
      credentials: "include",
      headers: csrfToken ? { 'X-XSRF-TOKEN': csrfToken } : undefined,
    });
  } catch {
    // Swallow network failures — the caller clears local state (query cache)
    // regardless, so logout always appears to succeed from the user's perspective.
  }
}

export function useLogin() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (credentials: LoginRequest): Promise<LoginResponse> => {
      try {
        const response = await fetch(`${API_BASE_URL}${endpoints.auth.login}`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          credentials: "include",
          body: JSON.stringify(credentials),
        });

        if (!response.ok) {
          throw new Error('Login failed. Please ensure the Email and Password is correct.');
        }
        
        return response.json();
      } catch (error) {
        // Network error (server down, no internet, etc.)
        if (error instanceof TypeError) {
          throw new Error('Failed to connect to server');
        }
        // Re-throw other errors
        throw error;
      }
    },
    onSuccess: (data) => {
      // The JWT itself arrives as an HttpOnly cookie (set by the server), not in this body
      queryClient.setQueryData(["currentUser"], data.user);
    },
  });
}

export function useSignup() {
  return useMutation({
    mutationFn: async (userData: SignupRequest) => {
      const response = await fetch(`${API_BASE_URL}${endpoints.auth.signup}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(userData),
      });
      
      if (!response.ok) {
        const body = await response.json().catch(() => null);
        throw new Error(body?.message || 'Signup failed');
      }

      return response.json();
    },
  });
}

export function useAuth() {
  const queryClient = useQueryClient();
  const { data: user, isLoading } = useQuery<User | null>({
    queryKey: ["currentUser"],
    queryFn: fetchUser,
    retry: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const logoutMutation = useMutation({
    mutationFn: logout,
    onSuccess: () => {
      queryClient.setQueryData(["currentUser"], null);
    },
  });

  return {
    user,
    isLoading,
    logout: logoutMutation.mutate,
    isLoggingOut: logoutMutation.isPending,
  };
}

export function useMarkWelcomeSeen() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (): Promise<void> => {
      await apiFetch(endpoints.user.markWelcomeSeen, { method: "PATCH" });
    },
    onSuccess: () => {
      queryClient.setQueryData<User | null>(["currentUser"], (prev) =>
        prev ? { ...prev, hasSeenWelcome: true } : prev,
      );
    },
  });
}
