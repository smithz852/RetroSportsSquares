// Frontend TypeScript types for .NET backend integration

export interface SquareGame {
  gameId: string;
  gameName: string;
  gameType: string;
  isOpen: boolean;
  pricePerSquare: number;
  squareSelectionLimit: number;
  hostUserId: string | null;
  isTurnBased: boolean;
  selectionPhaseActive: boolean;
  currentTurnUserId: string | null;
  turnTimeoutSeconds: number;
  turnStartedAt: string | null;
  playerCount: number;
  currentPlayerCount: number;
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
  homePeriodScores: number[];
  awayPeriodScores: number[];
  status: string;
  sportType: string;
  periodWinners: Record<number, string | null>;
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
  squareSelectionLimit: number | null;
  isTurnBased: boolean;
  turnTimeoutSeconds: number;
  dailySportsGameId: string;
  isPublic: boolean;
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

export interface AvailableSportLeague {
  sportType: string;
  league: string;
  leagueId: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface User {
  id: string;
  email: string;
  displayName: string;
  gamerTag: string | null;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface SignupRequest {
  name: string;
  email: string;
  password: string;
  gamerTag: string;
}

export interface TurnPlayer {
  userId: string;
  displayName: string;
  turnOrder: number;
  hasHadTurn: boolean;
  isHost: boolean;
}

export interface TurnStatus {
  selectionPhaseActive: boolean;
  currentTurnUserId: string | null;
  turnStartedAt: string | null;
  turnTimeoutSeconds: number;
  players: TurnPlayer[];
}

export interface PlayerStats {
  periodsWon: number;
  totalWagered: number;
  wagersWon: number;
  winRate: number;
  totalSquaresClaimed: number;
}

export interface CurrentGameSummary {
  gameId: string;
  gameName: string;
  gameType: string;
  pricePerSquare: number;
  squaresClaimed: number;
  isHost: boolean;
  isOpen: boolean;
  selectionPhaseActive: boolean;
}

export interface PastGameSummary {
  gameId: string;
  gameName: string;
  gameType: string;
  pricePerSquare: number;
  squaresClaimed: number;
  periodsWon: number;
  totalWagered: number;
  totalWon: number;
  createdAt: string;
}

export interface PaginatedPastGames {
  games: PastGameSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}
