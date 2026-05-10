// Frontend API client configuration for .NET backend
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7187';

// API endpoints for .NET backend
export const endpoints = {
  games: {
    list: '/SquareGames/GetAvailableSquareGames',
    create: '/SquareGames/CreateGame',
    options: (gameType: string, leagueId: string) =>  `/AvailableSportsGames/GetAvailable${gameType}League${leagueId}GameOptions`,
    scoreData: (gameId: string) => `/SquareGames/GetSquareGameScoreData/${gameId}`,
    squareGameById: (gameId: string) => `/SquareGames/${gameId}`,
    start: (gameId: string) => `/SquareGames/Start/${gameId}`
  },
  selections: {
    create: (gameId: string) => `/SquareGames/SquareSelections/${gameId}`,
    setGameNumbers: (gameId: string) => `/SquareGames/SetOutsideSquareNumbers/${gameId}`,
    getGameNumbers: (gameId: string) => `/SquareGames/GetOutsideSquareNumbers/${gameId}`,
    get: (gameId: string) => `/SquareGames/GetAllSelectedSquares/${gameId}`,
    getBoard: (gameId: string) => `/SquareGames/GetGameboard/${gameId}`,
    // getWinners: (gameId: string) => `/SquareGames/GetQuarterWinners/${gameId}`
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
