// Frontend TypeScript types for .NET backend integration

export interface Game {
  gameId: number;
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
