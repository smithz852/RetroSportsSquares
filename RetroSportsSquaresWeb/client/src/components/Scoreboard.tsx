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
}

export function Scoreboard({ isVisible, gameName }: ScoreboardProps) {
  if (!isVisible) return null;

  // Filler data as requested
  const team1: TeamData = {
    name: "BUCS",
    score: 24,
    quarters: [7, 7, 3, 7]
  };
  const team2: TeamData = {
    name: "PANTHERS",
    score: 17,
    quarters: [0, 10, 0, 7]
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
          <span className="text-red-500 text-2xl mb-2">NS</span>
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
              <Trophy className="w-4 h-4 text-yellow-600 animate-pulse" />
            </div>
          </div>
        ))}
      </div>
    </motion.div>
  );
}
