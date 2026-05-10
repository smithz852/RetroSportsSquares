// Frontend TypeScript types for .NET backend integration

export interface SquareGame {
  gameId: string;
  gameName: string;
  gameType: string;
  isOpen: boolean;
  pricePerSquare: number;
  playerCount: number;
  createdAt: string;
  sportGameId: number;
  homeTeam: string;
  awayTeam: string;
  startTime: Date;
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
  winnerQ1?: string;      // Display name
  winnerQ1UserId?: string;    // For tracking wins, future dev
  winnerQ2?: string;
  winnerQ2UserId?: string;
  winnerQ3?: string;
  winnerQ3UserId?: string;
  winnerQ4?: string;
  winnerQ4UserId?: string;
}

export interface SquareSelection {
  id: string;
  // gameId: number; enable later...
  userId: string;
  squareId: string;
  selectedAt: Date;
}

export interface CreateSquareGameRequest {
  name: string;
  gameType: string;
  playerCount: number;
  isOpen: boolean;
  pricePerSquare: number | null;
  dailySportsGameId: string;
}

export interface BoardSquare {
  id: string;
  rowIndex: number;
  colIndex: number;
  displayName: string | null;
}

export interface CreateSquareSelectionRequest {
  selections: Array<{
    squareId: string;
  }>;
}

export interface OutsideSquare {
  gameId: string | null;
  topNumbers: number[];
  leftNumbers: number[];
}

export interface CreateOutsideSquareNumbersRequest { //Will be deleted 
  outsideSquares: Array<{
    squareName: string;
    squareValue: number;
  }>;
}

export interface OutsideSquareNumbersResponse {
    outsideSquares: OutsideSquare[];
}

export interface SquareSelectionResponse {
  selections:  SquareSelection[];
}

export interface QuarterWinners {
  quarterlyWinners: Record<number, string | null>; // quarter number to winner name
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
