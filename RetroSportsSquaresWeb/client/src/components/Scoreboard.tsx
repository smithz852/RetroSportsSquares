import { GetGameScoreData } from "@/hooks/use-games";
import { motion } from "framer-motion";
import { Trophy, Clock } from "lucide-react";

interface TeamData {
  name: string;
  score: number;
  quarters: number[];
}

interface ScoreboardProps {
  isVisible: boolean;
  gameName?: string;
  squareGameId: string;
}

export function Scoreboard({ isVisible, gameName, squareGameId }: ScoreboardProps) {
  const { data: scoreData, isLoading, error } = GetGameScoreData(squareGameId);
  // console.log(scoreData)
  if (!isVisible) return null;
  
  // Show loading state
  if (isLoading) {
    return (
      <div className="w-full max-w-4xl mx-auto mb-8 bg-black border-4 border-red-900 p-8 text-center">
        <div className="text-red-500 font-pixel">LOADING SCORES...</div>
      </div>
    );
  }

  let q2HomeScore = (scoreData?.q1HomeScore ?? 0) + (scoreData?.q2HomeScore ?? 0);
  let q3HomeScore = q2HomeScore + (scoreData?.q3HomeScore ?? 0);
  let q4HomeScore = q3HomeScore + (scoreData?.q4HomeScore ?? 0);
  let q2AwayScore = (scoreData?.q1AwayScore ?? 0) + (scoreData?.q2AwayScore ?? 0);
  let q3AwayScore = q2AwayScore + (scoreData?.q3AwayScore ?? 0);
  let q4AwayScore = q3AwayScore + (scoreData?.q4AwayScore ?? 0);

  // Use scoreData if available, otherwise show placeholder
  const team1: TeamData = scoreData ? {
    name: scoreData.homeTeamName,
    score: scoreData.currentHomeScore,
    quarters: [
      scoreData.q1HomeScore ?? 0,
      q2HomeScore,
      q3HomeScore,
      q4HomeScore
    ]
  } : {
    name: "HOME",
    score: 0,
    quarters: [0, 0, 0, 0]
  };
  
  const team2: TeamData = scoreData ? {
    name: scoreData.awayTeamName,
    score: scoreData.currentAwayScore,
    quarters: [
      scoreData.q1AwayScore ?? 0,
      q2AwayScore,
      q3AwayScore,
      q4AwayScore
    ]
  } : {
    name: "AWAY",
    score: 0,
    quarters: [0, 0, 0, 0]
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: -20 }}
      animate={{ opacity: 1, y: 0 }}
      className="w-full max-w-4xl mx-auto mb-8 bg-black border-4 border-red-900 shadow-[0_0_30px_rgba(255,0,0,0.3)] font-pixel"
    >
      {/* Header */}
      <div className="border-b-4 border-red-900 p-2 text-center bg-red-900/10">
        <h2 className="text-red-600 text-2xl tracking-tighter uppercase">Score Board</h2>
        {gameName && <p className="text-red-900 text-[10px] uppercase mt-1">{gameName}</p>}
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
          <span className="text-red-500 text-2xl mb-2">{scoreData?.status}</span>
          <div className="flex flex-col items-center gap-1">
            <span className="text-red-900 text-[8px] uppercase">Kickoff Time</span>
            <div className="flex items-center gap-2 text-red-500 text-xl">
              <Clock className="w-4 h-4" />
              <span>13:08</span>
            </div>
          </div>
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
          <div key={q} className={`p-4 flex flex-col items-center justify-center ${idx < 3 ? 'border-r-4 border-red-900' : ''}`}>
            <span className="text-red-900 text-[10px] mb-2 uppercase">Q{q}</span>
            <div className="flex flex-col items-center gap-2">
              <span className="text-red-500 text-xs">
                {team1.quarters[idx]} - {team2.quarters[idx]}
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
