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
import { type PayoutMode } from "@shared/schema";

const PAYOUT_MODES: {
  value: PayoutMode;
  label: string;
  description: string;
  available: boolean;
}[] = [
  {
    value: "Default",
    label: "DEFAULT",
    description: "Even payout per period. Unclaimed periods refund everyone at the end.",
    available: true,
  },
  {
    value: "Fair",
    label: "FAIR",
    description: "Missed periods raise the payout of every winning period.",
    available: true,
  },
  {
    value: "Push",
    label: "PUSH",
    description: "Missed periods push their coins onto the next period's prize.",
    available: true,
  },
  {
    value: "Thief",
    label: "THIEF",
    description: "A missed period arms an arrow — the next winner gets robbed.",
    available: false,
  },
  {
    value: "Destruction",
    label: "DESTRUCTION",
    description: "A missed period bombs the previous winner's coins.",
    available: false,
  },
];

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
  const [squareSelectionLimit, setSquareSelectionLimit] = useState("");
  const [isTurnBased, setIsTurnBased] = useState(false);
  const [turnTimeoutSeconds, setTurnTimeoutSeconds] = useState(60);
  const [isPublic, setIsPublic] = useState(true);
  const [payoutMode, setPayoutMode] = useState<PayoutMode>("Default");
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
        isOpen: true,
        dailySportsGameId,
        pricePerSquare: parseFloat(wagerAmount) || 0,
        squareSelectionLimit: parseInt(squareSelectionLimit) || 0,
        isTurnBased,
        turnTimeoutSeconds,
        isPublic,
        payoutMode,
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
          <div className="space-y-2">
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">
              SQUARE SELECTION LIMIT
            </label>
            <input
              value={squareSelectionLimit}
              onChange={(e) => setSquareSelectionLimit(e.target.value)}
              placeholder="MAX SQUARES PER PLAYER (OPTIONAL)"
              className="w-full bg-black border-2 border-primary p-3 text-white font-['VT323'] text-xl focus:outline-none focus:ring-2 focus:ring-white placeholder:text-gray-700"
            />
          </div>
          <div className="space-y-2">
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">
              SELECTION MODE
            </label>
            <div className="flex gap-6">
              <button
                type="button"
                onClick={() => setIsTurnBased(false)}
                className="flex items-center gap-2 group"
              >
                <div className={`w-5 h-5 rounded-full border-2 border-primary flex items-center justify-center transition-colors ${!isTurnBased ? "bg-primary" : "bg-black"}`}>
                  {!isTurnBased && <div className="w-2 h-2 rounded-full bg-black" />}
                </div>
                <span className={`font-['Press_Start_2P'] text-xs ${!isTurnBased ? "text-primary" : "text-gray-500"}`}>
                  OPEN
                </span>
              </button>
              <button
                type="button"
                onClick={() => setIsTurnBased(true)}
                className="flex items-center gap-2 group"
              >
                <div className={`w-5 h-5 rounded-full border-2 border-primary flex items-center justify-center transition-colors ${isTurnBased ? "bg-primary" : "bg-black"}`}>
                  {isTurnBased && <div className="w-2 h-2 rounded-full bg-black" />}
                </div>
                <span className={`font-['Press_Start_2P'] text-xs ${isTurnBased ? "text-primary" : "text-gray-500"}`}>
                  TURN-BASED
                </span>
              </button>
            </div>
          </div>

          {isTurnBased && (
            <div className="space-y-2">
              <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">
                TURN TIMEOUT
              </label>
              <select
                value={turnTimeoutSeconds}
                onChange={(e) => setTurnTimeoutSeconds(parseInt(e.target.value))}
                className="w-full bg-black border-2 border-primary p-3 text-white font-['VT323'] text-xl focus:outline-none focus:ring-2 focus:ring-white"
              >
                <option value={0}>None</option>
                <option value={60}>1 min</option>
                <option value={120}>2 min</option>
                <option value={180}>3 min</option>
              </select>
            </div>
          )}

          <div className="space-y-2">
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">
              VISIBILITY
            </label>
            <div className="flex gap-6">
              <button
                type="button"
                onClick={() => setIsPublic(true)}
                className="flex items-center gap-2 group"
              >
                <div className={`w-5 h-5 rounded-full border-2 border-primary flex items-center justify-center transition-colors ${isPublic ? "bg-primary" : "bg-black"}`}>
                  {isPublic && <div className="w-2 h-2 rounded-full bg-black" />}
                </div>
                <span className={`font-['Press_Start_2P'] text-xs ${isPublic ? "text-primary" : "text-gray-500"}`}>
                  PUBLIC
                </span>
              </button>
              <button
                type="button"
                onClick={() => setIsPublic(false)}
                className="flex items-center gap-2 group"
              >
                <div className={`w-5 h-5 rounded-full border-2 border-primary flex items-center justify-center transition-colors ${!isPublic ? "bg-primary" : "bg-black"}`}>
                  {!isPublic && <div className="w-2 h-2 rounded-full bg-black" />}
                </div>
                <span className={`font-['Press_Start_2P'] text-xs ${!isPublic ? "text-primary" : "text-gray-500"}`}>
                  PRIVATE
                </span>
              </button>
            </div>
          </div>
          <div className="space-y-2">
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">
              GAME MODE
            </label>
            <div className="space-y-1">
              {PAYOUT_MODES.map((mode) => (
                <button
                  key={mode.value}
                  type="button"
                  disabled={!mode.available}
                  onClick={() => mode.available && setPayoutMode(mode.value)}
                  className={`w-full flex items-start gap-3 border-2 p-2 text-left transition-colors ${
                    payoutMode === mode.value
                      ? "border-primary bg-primary/10"
                      : "border-primary/30"
                  } ${!mode.available ? "opacity-50 cursor-not-allowed" : "hover:border-primary"}`}
                >
                  <div className={`mt-0.5 w-4 h-4 shrink-0 rounded-full border-2 border-primary flex items-center justify-center ${payoutMode === mode.value ? "bg-primary" : "bg-black"}`}>
                    {payoutMode === mode.value && <div className="w-1.5 h-1.5 rounded-full bg-black" />}
                  </div>
                  <div>
                    <span className={`font-['Press_Start_2P'] text-xs ${payoutMode === mode.value ? "text-primary" : "text-gray-400"}`}>
                      {mode.label}
                      {!mode.available && <span className="ml-2 text-yellow-400">SOON</span>}
                    </span>
                    <p className="text-gray-400 font-['VT323'] text-base leading-tight mt-1">
                      {mode.description}
                    </p>
                  </div>
                </button>
              ))}
            </div>
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
