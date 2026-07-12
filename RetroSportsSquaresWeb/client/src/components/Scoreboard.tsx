import { motion } from "framer-motion";
import { Trophy, Clock } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

interface TeamData {
  name: string;
  score: number;
  periodScores: (number | null | undefined)[];
}

interface ScoreboardProps {
  isVisible: boolean;
  gameName?: string;
  scoreData: any;
  isLoading: boolean;
  gameStartTime: Date | undefined;
  currentPeriod: number;
  currentLeader: string | null;
  periodWinners: Record<number, string | null>;
}

const SPORT_PERIOD_MAPS: Record<string, Record<string, number>> = {
  basketball: { Q1: 1, Q2: 2, HALF: 3, HT: 3, Q3: 3, Q4: 4, FINAL: 5, FT: 5, OT: 5, AOT: 5 },
  "american-football": { Q1: 1, Q2: 2, HALF: 3, HT: 3, Q3: 3, Q4: 4, FINAL: 5, FT: 5, OT: 5, AOT: 5 },
  soccer: { "1H": 1, HT: 2, "2H": 2, ET: 2, BT: 2, FT: 3, AET: 3, PEN: 3 },
};

export const getCurrentGamePeriodIndex = (status?: string | null, sportType?: string | null): number => {
  if (!status) return 0;
  const upper = status.toUpperCase();
  const map = SPORT_PERIOD_MAPS[sportType?.toLowerCase() ?? ""] ?? SPORT_PERIOD_MAPS["basketball"];
  for (const key in map) {
    if (upper.includes(key)) return map[key];
  }
  return 0;
};

const getPeriodLabel = (sportType: string | undefined, periodIndex: number): string => {
  switch (sportType?.toLowerCase()) {
    case "soccer": return `H${periodIndex}`;
    case "baseball": return `I${periodIndex}`;
    default: return `Q${periodIndex}`;
  }
};

const getExpectedPeriods = (sportType: string | undefined): number => {
  switch (sportType?.toLowerCase()) {
    case "soccer": return 2;
    case "baseball": return 9;
    default: return 4;
  }
};

export function Scoreboard({
  isVisible,
  gameName,
  scoreData,
  isLoading,
  gameStartTime,
  currentPeriod,
  currentLeader,
  periodWinners,
}: ScoreboardProps) {

  const [hasGameStarted, setHasGameStarted] = useState(false);
  const [timeLeft, setTimeLeft] = useState<number | null>(null);

  const sportType = scoreData?.sportType as string | undefined;
  const expectedPeriods = getExpectedPeriods(sportType);
  const periodCount = Math.max(
    scoreData?.homePeriodScores?.length ?? 0,
    expectedPeriods
  );

  const timeUntilGame = () => {
    if (!gameStartTime) return <span>TBD</span>;
    if (timeLeft === null) return null;

    const totalMinutes = Math.floor(timeLeft / (1000 * 60));
    const hoursLeft = Math.floor(totalMinutes / 60);
    const minutesLeft = totalMinutes % 60;
    const secondsLeft = Math.floor((timeLeft % (1000 * 60)) / 1000);

    return (
      <span>
        {hoursLeft}h {minutesLeft}m {secondsLeft}s
      </span>
    );
  };

  useEffect(() => {
    if (!gameStartTime) return;

    setHasGameStarted(false);

    let interval: ReturnType<typeof setInterval>;

    const calculateTimeLeft = () => {
      const startTime = new Date(gameStartTime);
      const now = new Date();
      const diff = startTime.getTime() - now.getTime();

      if (diff <= 0) {
        setHasGameStarted(true);
        setTimeLeft(0);
        clearInterval(interval);
        return;
      }

      setTimeLeft(diff);
    };

    calculateTimeLeft();
    interval = setInterval(calculateTimeLeft, 1000);

    return () => clearInterval(interval);
  }, [gameStartTime]);

  const team1 = useMemo<TeamData>(
    () =>
      scoreData
        ? {
            name: scoreData.homeTeamName,
            score: scoreData.currentHomeScore,
            periodScores: scoreData.homePeriodScores ?? [],
          }
        : { name: "HOME", score: 0, periodScores: [] },
    [scoreData],
  );

  const team2 = useMemo<TeamData>(
    () =>
      scoreData
        ? {
            name: scoreData.awayTeamName,
            score: scoreData.currentAwayScore,
            periodScores: scoreData.awayPeriodScores ?? [],
          }
        : { name: "AWAY", score: 0, periodScores: [] },
    [scoreData],
  );

  const sumThrough = (scores: (number | null | undefined)[], through: number) =>
    scores.slice(0, through).reduce((sum: number, s) => sum + (s ?? 0), 0);

  const getScoreAt = (
    scores: (number | null | undefined)[],
    period: number,
    currentPeriod: number,
  ) => {
    if (period > currentPeriod) return null;
    return sumThrough(scores, period);
  };

  const team1Totals = useMemo(
    () => Array.from({ length: periodCount }, (_, i) => getScoreAt(team1.periodScores, i + 1, currentPeriod)),
    [team1.periodScores, currentPeriod, periodCount],
  );

  const team2Totals = useMemo(
    () => Array.from({ length: periodCount }, (_, i) => getScoreAt(team2.periodScores, i + 1, currentPeriod)),
    [team2.periodScores, currentPeriod, periodCount],
  );

  if (!isVisible) return null;

  if (isLoading) {
    return (
      <div className="w-full max-w-2xl mx-auto bg-black border-4 border-red-900 p-8 text-center">
        <div className="text-red-500 font-pixel">LOADING SCORES...</div>
      </div>
    );
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: -20 }}
      animate={{ opacity: 1, y: 0 }}
      className="w-full max-w-2xl mx-auto bg-black border-4 border-red-900 shadow-[0_0_30px_rgba(255,0,0,0.3)] font-pixel"
    >
      {/* Header */}
      <div className="border-b-4 border-red-900 p-2 text-center bg-red-900/10">
        <h2 className="text-red-600 text-xl tracking-tighter uppercase">
          Score Board
        </h2>
        {gameName && (
          <p className="text-red-900 text-[10px] uppercase mt-1">{gameName}</p>
        )}
      </div>

      {/* Main Score Area */}
      <div className="grid grid-cols-3 border-b-4 border-red-900">
        <div className="p-3 flex flex-col items-center justify-center border-r-4 border-red-900 bg-black">
          <span className="text-red-500 text-base mb-2">{team1.name}</span>
          <span className="text-red-600 text-4xl font-mono">{team1.score}</span>
        </div>

        <div className="p-3 flex flex-col items-center justify-center bg-red-900/5">
          <span className="text-red-500 text-xl mb-2">
            {scoreData?.status}
          </span>
          {hasGameStarted ? (
            <div className="flex flex-col items-center gap-1">
              <span className="text-red-900 text-[8px] uppercase">Leader</span>
              <div className="flex items-center gap-2 text-red-500 text-lg">
                <span>{currentLeader}</span>
              </div>
            </div>
          ) : (
            <div className="flex flex-col items-center gap-1">
              <span className="text-red-900 text-[8px] uppercase">
                Game Time
              </span>
              <div className="flex items-center gap-2 text-red-500 text-lg">
                <Clock className="w-4 h-4" />
                {timeUntilGame()}
              </div>
            </div>
          )}
        </div>

        <div className="p-3 flex flex-col items-center justify-center border-l-4 border-red-900 bg-black">
          <span className="text-red-500 text-base mb-2">{team2.name}</span>
          <span className="text-red-600 text-4xl font-mono">{team2.score}</span>
        </div>
      </div>

      {/* Periods Grid — renders N columns based on sport */}
      <div className={`grid bg-black`} style={{ gridTemplateColumns: `repeat(${periodCount}, minmax(0, 1fr))` }}>
        {Array.from({ length: periodCount }, (_, idx) => {
          const periodNum = idx + 1;
          const winner = periodWinners[periodNum];
          return (
            <div
              key={periodNum}
              className={`p-1.5 sm:p-2 flex flex-col items-center justify-center ${idx < periodCount - 1 ? "border-r-4 border-red-900" : ""}`}
            >
              <span className="text-red-900 text-[10px] mb-2 uppercase">
                {getPeriodLabel(sportType, periodNum)}
              </span>
              <div className="flex flex-col items-center gap-2">
                <span className="text-red-500 text-xs">
                  {team1Totals[idx] ?? "-"} - {team2Totals[idx] ?? "-"}
                </span>
                {winner ? (
                  <div className="flex items-center gap-1">
                    <span className="text-yellow-600 text-[10px]">{winner}</span>
                  </div>
                ) : (
                  <Trophy className="w-4 h-4 text-yellow-600" />
                )}
              </div>
            </div>
          );
        })}
      </div>
    </motion.div>
  );
}
