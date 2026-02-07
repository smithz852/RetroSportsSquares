// Frontend TypeScript types for .NET backend integration

export interface SquareGame {
  gameId: string;
  gameName: string;
  gameType: 'football' | 'basketball';
  isOpen: boolean;
  pricePerSquare: number;
  createdAt: string;
  sportGameId: number;
  homeTeam: string;
  awayTeam: string;
}

export interface SquareGameScoreData {
    homeTeam: { name: string; score: number; quarterlyHomeScores: {Q1: number; Q2: number; Q3: number; Q4: number; OT: number } };
    awayTeam: { name: string; score: number; quarterlyAwayScores: {Q1: number; Q2: number; Q3: number; Q4: number; OT: number } };
    status: string;
    winnerQ1: User;
    winnerQ2: User;
    winnerQ3: User;
    winnerQ4: User;
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

export interface CreateSquareGameRequest {
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
  HomeTeam: string;
  AwayTeam: string;
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
