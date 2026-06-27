import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "@/hooks/use-auth";
import { useAvailableSportsAndLeagues } from "@/hooks/use-games";
import { Trophy, Circle, GraduationCap, Globe, Loader2 } from "lucide-react";
import { motion } from "framer-motion";
import { useEffect } from "react";
import { useLocation, useParams, Link } from "wouter";

const LEAGUE_UI: Record<string, {
  icon: React.ElementType;
  subtitle: string;
  animation: string;
}> = {
  NFL:        { icon: Trophy,        subtitle: "NATIONAL FOOTBALL LEAGUE",        animation: "animate-pulse" },
  NCAA:       { icon: GraduationCap, subtitle: "COLLEGE SPORTS",                  animation: "animate-bounce" },
  NBA:        { icon: Circle,        subtitle: "NATIONAL BASKETBALL ASSOCIATION",  animation: "animate-pulse" },
  "World Cup":{ icon: Globe,         subtitle: "FIFA WORLD CUP",                  animation: "animate-spin" },
};

const FALLBACK_LEAGUE_UI = { icon: Trophy, subtitle: "", animation: "animate-pulse" };

export default function LeagueOptions() {
  const [, setLocation] = useLocation();
  const { sport } = useParams<{ sport: string }>();
  const { user, isLoading: authLoading } = useAuth();
  const { data: sportsAndLeagues, isLoading: optionsLoading } = useAvailableSportsAndLeagues();

  useEffect(() => {
    if (!authLoading && !user) {
      setLocation("/login");
    }
  }, [user, authLoading]);

  if (authLoading || !user) return null;

  const leagues = sportsAndLeagues?.filter(s => s.sportType === sport) ?? [];

  const sportLabel = sport
    ? sport.charAt(0).toUpperCase() + sport.slice(1)
    : "";

  return (
    <div className="flex flex-col items-center justify-center min-h-[80vh] gap-12 p-4">
      <motion.div
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        className="flex flex-col items-center gap-2"
      >
        <h1 className="text-4xl md:text-6xl text-red-600 font-pixel text-center leading-tight uppercase tracking-tighter">
          {sportLabel} Leagues
        </h1>
        <p className="text-red-500/60 font-mono text-sm uppercase tracking-widest">
          SELECT YOUR LEAGUE
        </p>
      </motion.div>

      {optionsLoading ? (
        <div className="flex flex-col items-center gap-4">
          <Loader2 className="h-12 w-12 text-red-600 animate-spin" />
          <p className="text-red-600/60 font-pixel text-xs uppercase tracking-widest animate-pulse">
            Loading Leagues...
          </p>
        </div>
      ) : leagues.length === 0 ? (
        <div className="flex flex-col items-center gap-4 border-2 border-dashed border-red-900/40 p-12">
          <Trophy className="w-12 h-12 text-red-900/30" />
          <p className="text-red-900/60 font-pixel text-xs uppercase tracking-widest text-center">
            No {sportLabel} Leagues Scheduled Today
          </p>
        </div>
      ) : (
        <div className="flex flex-wrap justify-center gap-8 w-full max-w-4xl">
          {leagues.map((league, i) => {
            const ui = LEAGUE_UI[league.league] ?? FALLBACK_LEAGUE_UI;
            const Icon = ui.icon;
            return (
              <Link key={league.leagueId} href={`/arena/${sport}/${league.leagueId}`} asChild>
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: i * 0.1 }}
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                  className="cursor-pointer w-72"
                >
                  <Card className="bg-black border-4 border-red-600 rounded-none overflow-hidden group hover:shadow-[0_0_20px_rgba(255,0,0,0.5)] transition-all w-full h-full">
                    <CardContent className="p-12 flex flex-col items-center gap-6">
                      <div className={`text-red-600 group-hover:${ui.animation}`}>
                        <Icon size={80} />
                      </div>
                      <h2 className="text-3xl text-red-600 font-pixel uppercase tracking-widest text-center">
                        {league.league}
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

      <motion.button
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.3 }}
        onClick={() => setLocation("/options")}
        className="text-red-600/50 font-mono text-xs uppercase tracking-widest hover:text-red-500 transition-colors"
      >
        &lt; BACK TO SPORTS
      </motion.button>
    </div>
  );
}
