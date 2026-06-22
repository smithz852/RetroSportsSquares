import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "@/hooks/use-auth";
import { Trophy, Circle, GraduationCap, Globe } from "lucide-react";
import { motion } from "framer-motion";
import { useLocation, useParams, Link } from "wouter";

type LeagueConfig = {
  name: string;
  leagueId: number;
  icon: React.ElementType;
  subtitle: string;
  animation: string;
};

type SportConfig = {
  title: string;
  leagues: LeagueConfig[];
};

const SPORT_LEAGUES: Record<string, SportConfig> = {
  football: {
    title: "Football Leagues",
    leagues: [
      { name: "NFL", leagueId: 1, icon: Trophy, subtitle: "NATIONAL FOOTBALL LEAGUE", animation: "animate-pulse" },
      { name: "NCAA", leagueId: 11, icon: GraduationCap, subtitle: "COLLEGE FOOTBALL", animation: "animate-bounce" },
    ],
  },
  basketball: {
    title: "Basketball Leagues",
    leagues: [
      { name: "NBA", leagueId: 12, icon: Circle, subtitle: "NATIONAL BASKETBALL ASSOCIATION", animation: "animate-pulse" },
      { name: "NCAA", leagueId: 116, icon: GraduationCap, subtitle: "COLLEGE BASKETBALL", animation: "animate-bounce" },
    ],
  },
  soccer: {
    title: "Soccer Leagues",
    leagues: [
      { name: "World Cup", leagueId: 1, icon: Globe, subtitle: "FIFA WORLD CUP", animation: "animate-spin" },
    ],
  },
};

export default function LeagueOptions() {
  const [, setLocation] = useLocation();
  const { sport } = useParams<{ sport: string }>();
  const { user } = useAuth();

  if (!user) {
    setLocation("/login");
    return null;
  }

  const sportConfig = sport ? SPORT_LEAGUES[sport.toLowerCase()] : undefined;

  if (!sportConfig) {
    setLocation("/options");
    return null;
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-[80vh] gap-12 p-4">
      <motion.div
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        className="flex flex-col items-center gap-2"
      >
        <h1 className="text-4xl md:text-6xl text-red-600 font-pixel text-center leading-tight uppercase tracking-tighter">
          {sportConfig.title}
        </h1>
        <p className="text-red-500/60 font-mono text-sm uppercase tracking-widest">
          SELECT YOUR LEAGUE
        </p>
      </motion.div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 w-full max-w-4xl">
        {sportConfig.leagues.map((league, i) => {
          const Icon = league.icon;
          return (
            <Link key={league.name} href={`/arena/${sport}/${league.leagueId}`} asChild>
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: i * 0.1 }}
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                className="cursor-pointer"
              >
                <Card className="bg-black border-4 border-red-600 rounded-none overflow-hidden group hover:shadow-[0_0_20px_rgba(255,0,0,0.5)] transition-all">
                  <CardContent className="p-12 flex flex-col items-center gap-6">
                    <div className={`text-red-600 group-hover:${league.animation}`}>
                      <Icon size={80} />
                    </div>
                    <h2 className="text-3xl text-red-600 font-pixel uppercase tracking-widest text-center">
                      {league.name}
                    </h2>
                    <p className="text-red-500/70 font-mono text-sm text-center">{league.subtitle}</p>
                  </CardContent>
                </Card>
              </motion.div>
            </Link>
          );
        })}
      </div>

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
