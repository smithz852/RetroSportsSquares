// Frontend TypeScript types for .NET backend integration

export interface SquareGame {
  gameId: string;
  gameName: string;
  gameType: string;
  isOpen: boolean;
  pricePerSquare: number;
  createdAt: string;
  sportGameId: number;
  homeTeam: string;
  awayTeam: string;
}

export interface SquareGameScoreData {
  homeTeamName: string;
  awayTeamName: string;
  currentHomeScore: number;
  currentAwayScore: number;
  q1HomeScore: number;
  q2HomeScore: number;
  q3HomeScore: number;
  q4HomeScore: number;
  oTHomeScore: number;
  q1AwayScore: number;
  q2AwayScore: number;
  q3AwayScore: number;
  q4AwayScore: number;
  oTAwayScore: number;
  status: string;
  winnerQ1Name?: string;      // Display name
  winnerQ1UserId?: string;    // For tracking wins, future dev
  winnerQ2Name?: string;
  winnerQ2UserId?: string;
  winnerQ3Name?: string;
  winnerQ3UserId?: string;
  winnerQ4Name?: string;
  winnerQ4UserId?: string;
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
  gameType: string;
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
  homeTeam: string;
  awayTeam: string;
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
