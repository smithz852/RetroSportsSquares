import { useGames } from "@/hooks/use-games";
import { RetroCard } from "@/components/RetroCard";
import { CreateGameDialog } from "@/components/CreateGameDialog";
import { Loader2, Calendar, User, Trophy } from "lucide-react";
import { motion } from "framer-motion";
import { format } from "date-fns";
import { useLocation, useParams } from "wouter";

export default function Dashboard() {
  const { type } = useParams<{ type: string }>();
  const { data: allGames, isLoading, error } = useGames();
  const [, setLocation] = useLocation();

  const games = allGames?.filter(game => !type || game.type === type) || [];

  if (isLoading) {
    return (
      <div className="min-h-[80vh] flex flex-col items-center justify-center gap-4">
        <Loader2 className="h-16 w-16 text-primary animate-spin" />
        <p className="font-['Press_Start_2P'] text-primary animate-pulse">LOADING DATA...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-[80vh] flex items-center justify-center">
        <RetroCard className="border-red-600 text-center max-w-lg">
          <h2 className="font-['Press_Start_2P'] text-red-500 mb-4">SYSTEM ERROR</h2>
          <p className="text-red-400 font-['VT323'] text-xl">FAILED TO LOAD GAMES DATABASE.</p>
        </RetroCard>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 border-b-2 border-primary/30 pb-6">
        <div>
          <h1 className="text-2xl md:text-3xl font-['Press_Start_2P'] text-primary mb-2 text-shadow-retro uppercase">
            {type ? `${type} Games` : 'Active Games'}
          </h1>
          <p className="text-gray-400 font-['VT323'] text-xl">SELECT A CHALLENGE TO BEGIN</p>
        </div>
        <CreateGameDialog />
      </div>

      {games?.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 border-2 border-dashed border-primary/30">
          <Trophy className="w-16 h-16 text-primary/20 mb-4" />
          <p className="font-['Press_Start_2P'] text-gray-500 mb-6 text-center uppercase">NO {type ? type : ''} GAMES FOUND</p>
          <CreateGameDialog />
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {games?.map((game, i) => (
            <motion.div
              key={game.id}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1 }}
              onClick={() => setLocation(`/game/${game.id}`)}
            >
              <RetroCard title={`ID: ${game.id.toString().padStart(4, '0')}`} className="h-full hover:border-white transition-colors cursor-pointer group">
                <div className="flex justify-between items-start mb-4">
                  <h3 className="font-['Press_Start_2P'] text-white text-sm leading-6 line-clamp-2 group-hover:text-primary transition-colors">
                    {game.name}
                  </h3>
                  <span className={`px-2 py-1 text-xs font-['Press_Start_2P'] ${
                    game.status === 'open' ? 'bg-green-900 text-green-400' : 'bg-red-900 text-red-400'
                  }`}>
                    {game.status}
                  </span>
                </div>
                
                <div className="space-y-2 mt-4 pt-4 border-t-2 border-primary/20">
                  <div className="flex items-center text-gray-400 text-sm font-['VT323'] text-lg">
                    <Calendar className="w-4 h-4 mr-2" />
                    {game.createdAt ? format(new Date(game.createdAt), 'MMM dd, yyyy') : 'UNKNOWN'}
                  </div>
                  <div className="flex items-center text-gray-400 text-sm font-['VT323'] text-lg">
                    <User className="w-4 h-4 mr-2" />
                    Players: {(game as any).playerCount || 0}/100
                  </div>
                </div>
              </RetroCard>
            </motion.div>
          ))}
        </div>
      )}
    </div>
  );
}
