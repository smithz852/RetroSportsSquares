import { GetGameScoreData } from "@/hooks/use-games";
import { motion } from "framer-motion";
import { Trophy, Clock } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { start } from "repl";

interface TeamData {
  name: string;
  score: number;
  quarters: (number | null | undefined)[];
}

interface ScoreboardProps {
  isVisible: boolean;
  gameName?: string;
  squareGameId: string;
  gameStartTime: Date | undefined;
}

export function Scoreboard({
  isVisible,
  gameName,
  squareGameId,
  gameStartTime,
}: ScoreboardProps) {
  const {
    data: scoreData,
    isLoading,
    error,
  } = GetGameScoreData(squareGameId, isVisible ? 2 * 60 * 1000 : false);
  // console.log("GST: ", gameStartTime);

  const [hasGameStarted, setHasGameStarted] = useState(false);
  const [timeLeft, setTimeLeft] = useState<number | null>(null);

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

  const calculateTimeLeft = () => {
    const startTime = new Date(gameStartTime);
    // console.log("ST", startTime)
    const now = new Date();

    const diff = startTime.getTime() - now.getTime();
// console.log(diff)
    if (diff <= 0) {
      setHasGameStarted(true);
      setTimeLeft(0);
      return;
    }

    setTimeLeft(diff);
  };

  calculateTimeLeft();
  const interval = setInterval(calculateTimeLeft, 1000);

  return () => clearInterval(interval);

}, [gameStartTime]);

  // Use scoreData if available, otherwise show placeholder
  const team1 = useMemo<TeamData>(
    () =>
      scoreData
        ? {
            name: scoreData.homeTeamName,
            score: scoreData.currentHomeScore,
            quarters: [
              scoreData?.q1HomeScore,
              scoreData?.q2HomeScore,
              scoreData?.q3HomeScore,
              scoreData?.q4HomeScore,
              // scoreData?.oTHomeScore,
            ],
          }
        : {
            name: "HOME",
            score: 0,
            quarters: [null, null, null, null],
          },
    [scoreData],
  );

  const team2 = useMemo<TeamData>(
    () =>
      scoreData
        ? {
            name: scoreData.awayTeamName,
            score: scoreData.currentAwayScore,
            quarters: [
              scoreData?.q1AwayScore,
              scoreData?.q2AwayScore,
              scoreData?.q3AwayScore,
              scoreData?.q4AwayScore,
              // scoreData?.oTAwayScore,
            ],
          }
        : {
            name: "AWAY",
            score: 0,
            quarters: [null, null, null, null],
          },
    [scoreData],
  );

  const PERIOD_MAP: Record<string, number> = {
    Q1: 1,
    Q2: 2,
    HALF: 2,
    Q3: 3,
    Q4: 4,
    FINAL: 4,
    FT: 4,
    OT: 5,
  };

  const getCurrentGamePeriodIndex = (period?: string | null): number => {
    if (!period) return 0;

    const upper = period.toUpperCase();

    for (const key in PERIOD_MAP) {
      if (upper.includes(key)) return PERIOD_MAP[key];
    }

    return 0;
  };

  const currentQuarter = getCurrentGamePeriodIndex(scoreData?.status);

  const sumThroughQuarter = (
    quarters: (number | null | undefined)[],
    throughQuarter: number,
  ) =>
    quarters
      .slice(0, throughQuarter)
      .reduce((sum: number, q) => sum + (q ?? 0), 0);

  const getScoreAtQuarter = (
    quarters: (number | null | undefined)[],
    quarter: number,
    currentQuarter: number,
  ) => {
    if (quarter > currentQuarter) return null;

    return sumThroughQuarter(quarters, quarter);
  };

  const buildTotalsByQuarter = (
    quarters: (number | null | undefined)[],
    currentQuarter: number,
  ) =>
    quarters.map((_, i) => getScoreAtQuarter(quarters, i + 1, currentQuarter));

  const team1Totals = useMemo(
    () => buildTotalsByQuarter(team1.quarters, currentQuarter),
    [team1.quarters, currentQuarter],
  );

  const team2Totals = useMemo(
    () => buildTotalsByQuarter(team2.quarters, currentQuarter),
    [team2.quarters, currentQuarter],
  );

  if (!isVisible) return null;

  // Show loading state
  if (isLoading) {
    return (
      <div className="w-full max-w-4xl mx-auto mb-8 bg-black border-4 border-red-900 p-8 text-center">
        <div className="text-red-500 font-pixel">LOADING SCORES...</div>
      </div>
    );
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: -20 }}
      animate={{ opacity: 1, y: 0 }}
      className="w-full max-w-4xl mx-auto mb-8 bg-black border-4 border-red-900 shadow-[0_0_30px_rgba(255,0,0,0.3)] font-pixel"
    >
      {/* Header */}
      <div className="border-b-4 border-red-900 p-2 text-center bg-red-900/10">
        <h2 className="text-red-600 text-2xl tracking-tighter uppercase">
          Score Board
        </h2>
        {gameName && (
          <p className="text-red-900 text-[10px] uppercase mt-1">{gameName}</p>
        )}
      </div>

      {/* Main Score Area */}
      <div className="grid grid-cols-3 border-b-4 border-red-900">
        {/* Team 1 */}
        <div className="p-4 flex flex-col items-center justify-center border-r-4 border-red-900 bg-black">
          <span className="text-red-500 text-lg mb-2">{team1.name}</span>
          <span className="text-red-600 text-5xl font-mono">{team1.score}</span>
        </div>

        {/* Center Info */}
        <div className="p-4 flex flex-col items-center justify-center bg-red-900/5">
          <span className="text-red-500 text-2xl mb-2">
            {scoreData?.status}
          </span>
          {hasGameStarted ? (
            <div className="flex flex-col items-center gap-1">
              <span className="text-red-900 text-[8px] uppercase">Leader</span>
              <div className="flex items-center gap-2 text-red-500 text-xl">
                <span>User2</span>
              </div>
            </div>
          ) : (
            <div className="flex flex-col items-center gap-1">
              <span className="text-red-900 text-[8px] uppercase">
                Game Time
              </span>
              <div className="flex items-center gap-2 text-red-500 text-xl">
                <Clock className="w-4 h-4" />
                {timeUntilGame()}
              </div>
            </div>
          )}
        </div>

        {/* Team 2 */}
        <div className="p-4 flex flex-col items-center justify-center border-l-4 border-red-900 bg-black">
          <span className="text-red-500 text-lg mb-2">{team2.name}</span>
          <span className="text-red-600 text-5xl font-mono">{team2.score}</span>
        </div>
      </div>

      {/* Quarters Grid */}
      <div className="grid grid-cols-4 bg-black">
        {[1, 2, 3, 4].map((q, idx) => (
          <div
            key={q}
            className={`p-4 flex flex-col items-center justify-center ${idx < 3 ? "border-r-4 border-red-900" : ""}`}
          >
            <span className="text-red-900 text-[10px] mb-2 uppercase">
              Q{q}
            </span>
            <div className="flex flex-col items-center gap-2">
              <span className="text-red-500 text-xs">
                {team1Totals[idx] ?? "-"} - {team2Totals[idx] ?? "-"}
              </span>
              {/* winners go here */}
              <Trophy className="w-4 h-4 text-yellow-600 animate-pulse" />
            </div>
          </div>
        ))}
      </div>
    </motion.div>
  );
}
