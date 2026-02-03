// Frontend TypeScript types for .NET backend integration

export interface Game {
  gameId: string;
  gameName: string;
  gameType: 'football' | 'basketball';
  status: 'open' | 'active' | 'started';
  pricePerSquare: number;
  topNumbers?: number[];
  leftNumbers?: number[];
  scoreData?: {
    team1: { name: string; score: number; quarters: number[] };
    team2: { name: string; score: number; quarters: number[] };
    currentQuarter: number;
    statusText: string;
  };
  createdAt: string;
}

export interface SquareSelection {
  id: number;
  gameId: number;
  userId: string;
  row: number;
  col: number;
  playerName: string;
  createdAt: string;
}

export interface CreateGameRequest {
  name: string;
  gameType: 'football' | 'basketball';
  playerCount: number;
  Status: 'open' | 'closed';
  pricePerSquare?: number;
  dailySportsGameId: string;
}

export interface CreateSquareSelectionRequest {
  userId: string;
  row: number;
  col: number;
  playerName: string;
}

export interface AvailableGameOptions {
  id: string;
  gameName: string;
  status: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface User {
  id: string;
  email: string;
  displayName: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface SignupRequest {
  name: string;
  email: string;
  password: string;
}
