import { useState } from "react";
import { useCreateGame, useGetAvailableGameOptions } from "@/hooks/use-games";
import { RetroButton } from "./RetroButton";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Plus } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { Button } from "react-day-picker";
import { useParams, useLocation } from "wouter";
import { Loader2 } from "lucide-react";

// Format game name for display
function formatGameName(HomeTeam: string, AwayTeam: string) {


  return (
    <>
      {HomeTeam} <br /> VS <br /> {AwayTeam}
    </>
  );
}

export function CreateGameDialog() {
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [playerCount, setPlayerCount] = useState("");
  const [dailySportsGameId, setDailySportsGameId] = useState("");
  const [wagerAmount, setWagerAmount] = useState("");
  const gameStatus = "open";
  const { mutate, isPending } = useCreateGame();
  const { toast } = useToast();
  const [, setLocation] = useLocation();
  const { type, leagueId } = useParams<{ type: string, leagueId: string }>();
  const {
    data: availableGames,
    isLoading,
    error,
  } = useGetAvailableGameOptions(type, leagueId);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    mutate(
      {
        name,
        gameType: type,
        playerCount: parseInt(playerCount),
        Status: gameStatus,
        dailySportsGameId,
        pricePerSquare: parseInt(wagerAmount) || 0,
      },
      {
        onSuccess: (createdGame) => {
          setOpen(false);
          setName("");
          setPlayerCount("");
          setWagerAmount("");
          toast({
            title: "GAME CREATED",
            description: "NEW CHALLENGE INITIALIZED.",
            className:
              "bg-black border-2 border-primary text-primary font-['VT323']",
          });
          // Navigate to the newly created game
          setLocation(`/game/${createdGame.gameId}`);
        },
        onError: () => {
          toast({
            title: "ERROR",
            description: "FAILED TO CREATE GAME.",
            variant: "destructive",
            className:
              "bg-black border-2 border-red-900 text-red-500 font-['VT323']",
          });
        },
      },
    );
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <RetroButton
          variant="primary"
          size="sm"
          className="flex items-center gap-2"
        >
          <Plus className="w-4 h-4" /> NEW GAME
        </RetroButton>
      </DialogTrigger>
      <DialogContent className="bg-black border-4 border-primary sm:max-w-md max-h-[95vh] overflow-y-auto p-0">
        <DialogHeader className="bg-primary/10 p-4 sm:p-6 border-b-2 border-primary">
          <DialogTitle className="font-['Press_Start_2P'] text-primary text-center text-xs sm:text-sm">
            NEW GAME
          </DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="p-4 sm:p-6 space-y-4 sm:space-y-6">
          <div className="space-y-2">
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">
              GAME TITLE
            </label>
            <input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="ENTER TITLE..."
              className="w-full bg-black border-2 border-primary p-3 text-white font-['VT323'] text-xl focus:outline-none focus:ring-2 focus:ring-white placeholder:text-gray-700"
              autoFocus
            />
          </div>
          <div className="space-y-2">
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">
              NUMBER OF PLAYERS
            </label>
            <input
              value={playerCount}
              onChange={(e) => setPlayerCount(e.target.value)}
              placeholder="ENTER VALUE BETWEEN 2-100"
              className="w-full bg-black border-2 border-primary p-3 text-white font-['VT323'] text-xl focus:outline-none focus:ring-2 focus:ring-white placeholder:text-gray-700"
              autoFocus
            />
          </div>
          <div className="space-y-2">
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">
              WAGER AMOUNT
            </label>
            <input
              value={wagerAmount}
              onChange={(e) => setWagerAmount(e.target.value)}
              placeholder="ENTER A VALUE PER SQUARE (OPTIONAL)"
              className="w-full bg-black border-2 border-primary p-3 text-white font-['VT323'] text-xl focus:outline-none focus:ring-2 focus:ring-white placeholder:text-gray-700"
              autoFocus
            />
          </div>
          <div>
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">
              SELECT A GAME
            </label>
            <div className="grid grid-cols-2 gap-1 max-h-[20vh] sm:max-h-[30vh] overflow-y-auto p-2">
              {isLoading ? (
                <div className="min-h-[80vh] flex flex-col items-center justify-center gap-4">
                  <Loader2 className="h-16 w-16 text-primary animate-spin" />
                  <p className="font-['Press_Start_2P'] text-primary animate-pulse">
                    LOADING DATA...
                  </p>
                </div>
              ) : error ? (
                <p className="text-red-400 font-['VT323'] text-xl">
                  Error loading games
                </p>
              ) : (
                availableGames?.map((game) => (
                  <div key={game.id}>
                    <h6>{game.status}</h6>
                    <RetroButton
                      type="button"
                      onClick={() => setDailySportsGameId(game.id)}
                      className={`p-1 m-2 ${dailySportsGameId === game.id ? "bg-primary" : ""}`}
                      size="sm"
                    >
                      {formatGameName(game.homeTeam, game.awayTeam)}
                    </RetroButton>
                  </div>
                ))
              )}
            </div>
          </div>
          <div className="flex justify-end pt-4">
            <RetroButton
              type="submit"
              disabled={isPending || !name.trim()}
              className="w-full sm:w-auto"
            >
              {isPending ? "INITIALIZING..." : "START GAME"}
            </RetroButton>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
