// Frontend API client configuration for .NET backend
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7187';

// API endpoints for .NET backend
export const endpoints = {
  games: {
    list: '/SquareGames/GetAvailableSquareGames',
    create: '/SquareGames/CreateGame',
    options: (gameType: string, leagueId: string) =>  `/AvailableSportsGames/GetAvailable/${gameType}/league/${leagueId}/GameOptions`,
    availableSportsAndLeagues: '/AvailableSportsGames/GetAvailableSportsAndLeagues',
    scoreData: (gameId: string) => `/SquareGames/GetSquareGameScoreData/${gameId}`,
    squareGameById: (gameId: string) => `/SquareGames/${gameId}`,
    start: (gameId: string) => `/SquareGames/Start/${gameId}`,
    join: (gameId: string) => `/SquareGames/join/${gameId}`,
    leave: (gameId: string) => `/SquareGames/leave/${gameId}`,
    delete: (gameId: string) => `/SquareGames/${gameId}`,
    beginSelections: (gameId: string) => `/SquareGames/begin-selections/${gameId}`,
    skipPlayer: (gameId: string) => `/SquareGames/skip-player/${gameId}`,
    turnStatus: (gameId: string) => `/SquareGames/turn-status/${gameId}`,
    findByShortId: (shortId: string) => `/SquareGames/find/${shortId}`,
    chat: (gameId: string) => `/SquareGames/chat/${gameId}`
  },
  selections: {
    create: (gameId: string) => `/SquareGames/SquareSelections/${gameId}`,
    setGameNumbers: (gameId: string) => `/SquareGames/SetOutsideSquareNumbers/${gameId}`,
    getGameNumbers: (gameId: string) => `/SquareGames/GetOutsideSquareNumbers/${gameId}`,
    get: (gameId: string) => `/SquareGames/GetAllSelectedSquares/${gameId}`,
    getBoard: (gameId: string) => `/SquareGames/GetGameboard/${gameId}`,
    // getWinners: (gameId: string) => `/SquareGames/GetQuarterWinners/${gameId}`
  },
  dashboard: {
    stats: '/PlayerDashboard/stats',
    currentGames: '/PlayerDashboard/current-games',
    pastGames: (page: number, pageSize: number) => `/PlayerDashboard/past-games?page=${page}&pageSize=${pageSize}`,
  },
  admin: {
    summary: '/Admin/summary',
    currentGames: '/Admin/games/current',
    pastGames: (page: number, pageSize: number) => `/Admin/games/past?page=${page}&pageSize=${pageSize}`,
    playerStats: '/Admin/players/stats',
    users: '/Admin/users',
    gameChatLog: (gameId: string) => `/Admin/games/${gameId}/chat`,
  },
  auth: {
    login: '/Auth/login',
    logout: '/Auth/logout',
    getUser: '/Auth/me',
    signup: '/Auth/signup',
    forgotPassword: '/Auth/forgot-password',
    resetPassword: '/Auth/reset-password',
  },
  user: {
    updateDisplayName: '/User/display-name',
    updateGamerTag: '/User/gamer-tag',
    requestEmailChange: '/User/request-email-change',
    confirmEmailChange: '/User/confirm-email-change',
  }
};

// Helper to build full API URLs
export function buildApiUrl(endpoint: string): string {
  return `${API_BASE_URL}${endpoint}`;
}
