import { useAuth } from "@/hooks/use-auth";
import Landing from "./Landing";
import GameOptions from "./GameOptions";
import { Loader2 } from "lucide-react";

export default function Home() {
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="h-screen w-full flex flex-col items-center justify-center bg-black gap-6">
        <Loader2 className="h-16 w-16 text-primary animate-spin" />
        <h2 className="text-primary font-['Press_Start_2P'] text-xl animate-pulse">LOADING SYSTEM...</h2>
      </div>
    );
  }

  if (user) {
    return <GameOptions />;
  }

  return <Landing />;
}
