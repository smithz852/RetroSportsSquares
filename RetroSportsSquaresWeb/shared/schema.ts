// Frontend TypeScript types for .NET backend integration

export interface Game {
  id: number;
  name: string;
  type: 'football' | 'basketball';
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
  type: 'football' | 'basketball';
  pricePerSquare?: number;
}

export interface CreateSquareSelectionRequest {
  userId: string;
  row: number;
  col: number;
  playerName: string;
}
