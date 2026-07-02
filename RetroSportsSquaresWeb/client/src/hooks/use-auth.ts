import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { API_BASE_URL, endpoints } from "@shared/routes";
import { type LoginRequest, type User, type LoginResponse, type SignupRequest } from "@shared/schema";



async function fetchUser(): Promise<User | null> {
  const token = localStorage.getItem('token');
  if (!token) return null;
  
  const response = await fetch(`${API_BASE_URL}${endpoints.auth.getUser}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  if (!response.ok) return null;
  return response.json();
}

async function logout(): Promise<void> {
  const token = localStorage.getItem('token');
  // Clear the local token first so logout succeeds even if the server call fails
  localStorage.removeItem('token');
  if (token) {
    try {
      // Best-effort server-side revocation (rotates the security stamp,
      // invalidating this user's tokens on all devices)
      await fetch(`${API_BASE_URL}${endpoints.auth.logout}`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
      });
    } catch {
      // Local logout already happened; ignore network failures
    }
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
      // Store token and user data
      localStorage.setItem('token', data.token);
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
        throw new Error('Signup failed');
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
