// Frontend API client configuration for .NET backend
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7187';

// API endpoints for .NET backend
export const endpoints = {
  games: {
    list: '/AvailableGames/GetAvailableSquareGames',
    create: '/AvailableGames/CreateGame',
    options: '/AvailableSportsGames/GetAvailableNflGameOptions',
    scoreData: (gameId: string) => `/AvailableGames/GetSquareGameScoreData/${gameId}`,
    squareGameById: (gameId: string) => `/AvailableGames/${gameId}`
  },
  selections: {
    list: (gameId: string) => `/games/${gameId}/selections`,
    create: (gameId: string) => `/games/${gameId}/selections`,
    clear: (gameId: string) => `/games/${gameId}/selections`,
  },
  auth: {
    login: '/Auth/login',
    logout: '/Auth/logout',
    getUser: '/Auth/me',
    signup: '/Auth/signup',
  }
};

// Helper to build full API URLs
export function buildApiUrl(endpoint: string): string {
  return `${API_BASE_URL}${endpoint}`;
}
