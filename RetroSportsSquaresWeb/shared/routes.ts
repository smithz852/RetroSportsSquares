// Frontend API client configuration for .NET backend
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

// API endpoints for .NET backend
export const endpoints = {
  games: {
    list: '/games',
    get: (id: string) => `/games/${id}`,
    create: '/games',
    update: (id: string) => `/games/${id}`,
  },
  selections: {
    list: (gameId: string) => `/games/${gameId}/selections`,
    create: (gameId: string) => `/games/${gameId}/selections`,
    clear: (gameId: string) => `/games/${gameId}/selections`,
  },
};

// Helper to build full API URLs
export function buildApiUrl(endpoint: string): string {
  return `${API_BASE_URL}${endpoint}`;
}
