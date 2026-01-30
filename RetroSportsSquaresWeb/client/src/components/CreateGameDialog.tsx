import { useState } from "react";
import { useCreateGame } from "@/hooks/use-games";
import { RetroButton } from "./RetroButton";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Plus } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { Button } from "react-day-picker";

export function CreateGameDialog() {
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [playerCount, setPlayerCount] = useState("");
  const gameStatus = "open";
  const { mutate, isPending } = useCreateGame();
  const { toast } = useToast();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

   mutate({ name, gameType: "football", playerCount: parseInt(playerCount), Status: gameStatus }, {
      onSuccess: () => {
        setOpen(false);
        setName("");
        setPlayerCount("");
        toast({
          title: "GAME CREATED",
          description: "NEW CHALLENGE INITIALIZED.",
          className: "bg-black border-2 border-primary text-primary font-['VT323']",
        });
      },
      onError: () => {
        toast({
          title: "ERROR",
          description: "FAILED TO CREATE GAME.",
          variant: "destructive",
          className: "bg-black border-2 border-red-900 text-red-500 font-['VT323']",
        });
      }
    });
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <RetroButton variant="primary" size="sm" className="flex items-center gap-2">
          <Plus className="w-4 h-4" /> NEW GAME
        </RetroButton>
      </DialogTrigger>
      <DialogContent className="bg-black border-4 border-primary sm:max-w-md p-0 overflow-hidden">
        <DialogHeader className="bg-primary/10 p-6 border-b-2 border-primary">
          <DialogTitle className="font-['Press_Start_2P'] text-primary text-center">NEW GAME</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          <div className="space-y-2">
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">GAME TITLE</label>
            <input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="ENTER TITLE..."
              className="w-full bg-black border-2 border-primary p-3 text-white font-['VT323'] text-xl focus:outline-none focus:ring-2 focus:ring-white placeholder:text-gray-700"
              autoFocus
            />
          </div>
          <div className="space-y-2">
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">NUMBER OF PLAYERS</label>
            <input
              value={playerCount}
              onChange={(e) => setPlayerCount(e.target.value)}
              placeholder="ENTER VALUE BETWEEN 2-100"
              className="w-full bg-black border-2 border-primary p-3 text-white font-['VT323'] text-xl focus:outline-none focus:ring-2 focus:ring-white placeholder:text-gray-700"
              autoFocus
            />
          </div>
          <div>
            <label className="text-primary font-['Press_Start_2P'] text-xs block mb-2">SELECT A GAME</label>
            <div className="grid grid-cols-2 gap-1">
              <RetroButton className="p-1 m-2" size="sm">Houston Texans <br/>VS <br/>Las Vegas Raiders</RetroButton>
              <RetroButton className="p-1 m-2" size="sm">Houston Texans <br/>VS <br/>Las Vegas Raiders</RetroButton>
              <RetroButton className="p-1 m-2" size="sm">Oklahoma City Thunder <br/>VS <br/> Minnesota Timberwolves</RetroButton>
            
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
