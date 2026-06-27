import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "@/hooks/use-auth";
import { useAvailableSportsAndLeagues } from "@/hooks/use-games";
import { Link } from "wouter";
import { Trophy, Circle, Globe, Loader2 } from "lucide-react";
import { motion } from "framer-motion";
import { useEffect } from "react";
import { useLocation } from "wouter";

const SPORT_UI: Record<string, {
  label: string;
  icon: React.ElementType;
  animation: string;
  subtitle: string;
}> = {
  football: {
    label: "Football",
    icon: Trophy,
    animation: "group-hover:animate-pulse",
    subtitle: "CLASSIC GRIDIRON SQUARES",
  },
  basketball: {
    label: "Basketball",
    icon: Circle,
    animation: "group-hover:animate-bounce",
    subtitle: "HARDWOOD HOOPS CHALLENGE",
  },
  soccer: {
    label: "Soccer",
    icon: Globe,
    animation: "group-hover:animate-spin",
    subtitle: "GLOBAL FOOTBALL SQUARES",
  },
};

export default function GameOptions() {
  const [, setLocation] = useLocation();
  const { user, isLoading: authLoading } = useAuth();
  const { data: sportsAndLeagues, isLoading: optionsLoading } = useAvailableSportsAndLeagues();

  useEffect(() => {
    if (!authLoading && !user) {
      setLocation("/login");
    }
  }, [user, authLoading]);

  if (authLoading || !user) return null;

  const availableSports = sportsAndLeagues
    ? [...new Map(sportsAndLeagues.map(s => [s.sportType, s])).values()]
    : [];

  return (
    <div className="flex flex-col items-center justify-center min-h-[80vh] gap-12 p-4">
      <motion.h1
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        className="text-4xl md:text-6xl text-red-600 font-pixel text-center leading-tight uppercase tracking-tighter"
      >
        Select Your Arena
      </motion.h1>

      {optionsLoading ? (
        <div className="flex flex-col items-center gap-4">
          <Loader2 className="h-12 w-12 text-red-600 animate-spin" />
          <p className="text-red-600/60 font-pixel text-xs uppercase tracking-widest animate-pulse">
            Loading Today's Sports...
          </p>
        </div>
      ) : availableSports.length === 0 ? (
        <div className="flex flex-col items-center gap-4 border-2 border-dashed border-red-900/40 p-12">
          <Trophy className="w-12 h-12 text-red-900/30" />
          <p className="text-red-900/60 font-pixel text-xs uppercase tracking-widest text-center">
            No Games Scheduled Today
          </p>
        </div>
      ) : (
        <div className="flex flex-wrap justify-center gap-8 w-full max-w-5xl">
          {availableSports.map((sport, i) => {
            const ui = SPORT_UI[sport.sportType] ?? {
              label: sport.sportType,
              icon: Trophy,
              animation: "group-hover:animate-pulse",
              subtitle: "",
            };
            const Icon = ui.icon;
            return (
              <Link key={sport.sportType} href={`/leagues/${sport.sportType}`} asChild>
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: i * 0.1 }}
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                  className="cursor-pointer w-80"
                >
                  <Card className="bg-black border-4 border-red-600 rounded-none overflow-hidden group hover:shadow-[0_0_20px_rgba(255,0,0,0.5)] transition-all w-full h-full">
                    <CardContent className="p-12 flex flex-col items-center gap-6">
                      <div className={`text-red-600 ${ui.animation}`}>
                        <Icon size={80} />
                      </div>
                      <h2 className="text-3xl text-red-600 font-pixel uppercase tracking-widest text-center">
                        {ui.label}
                      </h2>
                      <p className="text-red-500/70 font-mono text-sm text-center">{ui.subtitle}</p>
                    </CardContent>
                  </Card>
                </motion.div>
              </Link>
            );
          })}
        </div>
      )}
    </div>
  );
}
