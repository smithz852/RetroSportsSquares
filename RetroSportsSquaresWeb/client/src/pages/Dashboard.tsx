import { useGames } from "@/hooks/use-games";
import { useAuth } from "@/hooks/use-auth";
import { RetroCard } from "@/components/RetroCard";
import { CreateGameDialog } from "@/components/CreateGameDialog";
import { Loader2, Calendar, User, Trophy, X } from "lucide-react";
import { motion } from "framer-motion";
import { format } from "date-fns";
import { useLocation, useParams } from "wouter";
import { useState } from "react";
import { useJoinGame } from "@/hooks/use-gameplay";
import { useDeleteGame } from "@/hooks/use-games";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";

export default function Dashboard() {
  const { type } = useParams<{ type: string }>();
  const { data: allGames, isLoading, error } = useGames();
  const [, setLocation] = useLocation();
  const { user } = useAuth();
  const { mutate: joinGame, isPending: isJoining } = useJoinGame();
  const { mutate: deleteGame } = useDeleteGame();
  const [joiningGameId, setJoiningGameId] = useState<string | null>(null);
  const [showFullModal, setShowFullModal] = useState(false);
  const [confirmDeleteId, setConfirmDeleteId] = useState<string | null>(null);

  function handleDeleteClick(e: React.MouseEvent, gameId: string) {
    e.stopPropagation();
    setConfirmDeleteId(gameId);
  }

  function confirmDelete() {
    if (!confirmDeleteId) return;
    deleteGame(confirmDeleteId, {
      onSuccess: () => setConfirmDeleteId(null),
      onError: () => setConfirmDeleteId(null),
    });
  }

  function handleGameClick(gameId: string) {
    setJoiningGameId(gameId);
    joinGame(gameId, {
      onSuccess: () => setLocation(`/game/${gameId}`),
      onError: () => {
        setJoiningGameId(null);
        setShowFullModal(true);
      },
    });
  }
  
    

  const games = allGames?.filter(game => !type || game.gameType === type) || [];

  function formatId(fullGameId: string) {
    var splitId = fullGameId.split('-')[0]
    return splitId;
  }
if (user) {
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
              key={game.gameId}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1 }}
              onClick={() => !isJoining && handleGameClick(game.gameId)}
            >
              <RetroCard title={`ID: ${formatId(game.gameId)}`} className={`h-full hover:border-white transition-colors cursor-pointer group ${joiningGameId === game.gameId ? "opacity-60 pointer-events-none" : ""}`}>
                <div className="flex justify-between items-start mb-4">
                  <h3 className="font-['Press_Start_2P'] text-white text-sm leading-6 line-clamp-2 group-hover:text-primary transition-colors">
                    {game.gameName}
                  </h3>
                  <div className="flex items-center gap-2 shrink-0">
                    <span className={`px-2 py-1 text-xs font-['Press_Start_2P'] ${
                      game.isOpen ? 'bg-green-900 text-green-400' : 'bg-red-900 text-red-400'
                    }`}>
                      {game.isOpen ? "Open" : "Closed"}
                    </span>
                    {game.hostUserId === user?.id && game.isOpen && (
                      <button
                        onClick={(e) => handleDeleteClick(e, game.gameId)}
                        className="text-red-600 hover:text-red-400 transition-colors p-0.5"
                        title="Delete game"
                      >
                        <X className="w-4 h-4" />
                      </button>
                    )}
                  </div>
                </div>
                
                <div className="space-y-2 mt-4 pt-4 border-t-2 border-primary/20">
                  <div className="flex items-center text-gray-400 text-sm font-['VT323'] text-lg">
                    <Calendar className="w-4 h-4 mr-2" />
                    {game.createdAt ? format(new Date(game.createdAt), 'MMM dd, yyyy') : 'UNKNOWN'}
                  </div>
                  <div className="flex items-center text-gray-400 text-sm font-['VT323'] text-lg">
                    <User className="w-4 h-4 mr-2" />
                    Players: {game.currentPlayerCount}/{game.playerCount}
                  </div>
                  {joiningGameId === game.gameId && (
                    <div className="flex items-center gap-2 text-primary font-['VT323'] text-lg pt-1">
                      <Loader2 className="w-4 h-4 animate-spin" />
                      JOINING...
                    </div>
                  )}
                </div>
              </RetroCard>
            </motion.div>
          ))}
        </div>
      )}

      <Dialog open={!!confirmDeleteId} onOpenChange={(open) => !open && setConfirmDeleteId(null)}>
        <DialogContent className="bg-black border-4 border-red-600 rounded-none text-red-500 font-pixel max-w-sm">
          <DialogHeader>
            <DialogTitle className="text-red-600 font-pixel text-lg uppercase tracking-widest">
              Delete Game?
            </DialogTitle>
            <DialogDescription className="text-red-400 font-['VT323'] text-xl pt-2">
              This will permanently delete the game and all its data. This cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="pt-2 flex gap-2">
            <Button
              onClick={() => setConfirmDeleteId(null)}
              className="bg-black text-red-600 font-pixel rounded-none hover:bg-red-900/20 border-2 border-red-600 uppercase flex-1"
            >
              Cancel
            </Button>
            <Button
              onClick={confirmDelete}
              className="bg-red-600 text-black font-pixel rounded-none hover:bg-red-500 border-b-4 border-red-900 active:border-b-0 active:translate-y-1 transition-all uppercase flex-1"
            >
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={showFullModal} onOpenChange={setShowFullModal}>
        <DialogContent className="bg-black border-4 border-red-600 rounded-none text-red-500 font-pixel max-w-sm">
          <DialogHeader>
            <DialogTitle className="text-red-600 font-pixel text-lg uppercase tracking-widest">
              Game Full
            </DialogTitle>
            <DialogDescription className="text-red-400 font-['VT323'] text-xl pt-2">
              This game has reached its player limit. Please choose or create another game.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="pt-2">
            <Button
              onClick={() => setShowFullModal(false)}
              className="bg-red-600 text-black font-pixel rounded-none hover:bg-red-500 border-b-4 border-red-900 active:border-b-0 active:translate-y-1 transition-all uppercase w-full"
            >
              OK
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
    }
   else
    {
      setLocation("/login")
    }
}
