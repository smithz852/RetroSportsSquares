import { useState } from "react";
import { Link, useLocation } from "wouter";
import { useAuth } from "@/hooks/use-auth";
import { usePlayerStats, useCurrentGames, usePastGames } from "@/hooks/use-dashboard";
import { RetroCard } from "@/components/RetroCard";
import { RetroButton } from "@/components/RetroButton";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Loader2, Trophy } from "lucide-react";
import { format } from "date-fns";

function StatBox({ label, value }: { label: string; value: string }) {
  return (
    <div className="border-2 border-primary/40 bg-black p-4 flex flex-col items-center gap-2 text-center">
      <span className="font-['VT323'] text-gray-400 text-lg tracking-widest uppercase">{label}</span>
      <span className="font-['Press_Start_2P'] text-primary text-xl text-shadow-retro">{value}</span>
    </div>
  );
}

function LoadingRow() {
  return (
    <div className="flex items-center justify-center gap-3 py-10">
      <Loader2 className="h-6 w-6 text-primary animate-spin" />
      <span className="font-['Press_Start_2P'] text-primary text-xs animate-pulse">LOADING...</span>
    </div>
  );
}

function EmptyState({ message }: { message: string }) {
  return (
    <div className="text-center py-12 border-2 border-dashed border-primary/20">
      <Trophy className="w-10 h-10 text-primary/20 mx-auto mb-3" />
      <p className="font-['Press_Start_2P'] text-gray-600 text-xs">{message}</p>
    </div>
  );
}

const PAGE_SIZE = 10;

export default function PlayerDashboard() {
  const { user } = useAuth();
  const [, setLocation] = useLocation();
  const [page, setPage] = useState(1);
  const [loaded, setLoaded] = useState({ current: false, past: false });

  const { data: stats, isLoading: statsLoading } = usePlayerStats();
  const { data: currentGames, isLoading: currentLoading } = useCurrentGames(loaded.current);
  const { data: pastGamesData, isLoading: pastLoading } = usePastGames(page, PAGE_SIZE, loaded.past);

  if (!user) {
    setLocation("/login");
    return null;
  }

  const handleTabChange = (value: string) => {
    const key = value as 'current' | 'past';
    if (!loaded[key]) {
      setLoaded(prev => ({ ...prev, [key]: true }));
    }
  };

  const formatWinRate = (rate: number) => {
    if (rate === 0) return ".000";
    return rate.toFixed(3).replace("0.", ".");
  };

  const totalPages = Math.ceil((pastGamesData?.totalCount ?? 0) / PAGE_SIZE);

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="border-b-2 border-primary/30 pb-6">
        <h1 className="text-xl md:text-2xl font-['Press_Start_2P'] text-primary text-shadow-retro mb-2">
          PLAYER DASHBOARD
        </h1>
        <p className="font-['VT323'] text-gray-400 text-xl tracking-widest">
          PLAYER: {user.displayName?.toUpperCase() ?? "UNKNOWN"}
        </p>
      </div>

      {/* Stats Panel */}
      <RetroCard title="PLAYER STATS">
        {statsLoading ? (
          <LoadingRow />
        ) : stats ? (
          <div className="pt-4 grid grid-cols-2 md:grid-cols-3 gap-4">
            <StatBox label="PERIODS WON" value={String(stats.periodsWon)} />
            <StatBox label="WIN RATE" value={formatWinRate(stats.winRate)} />
            <StatBox label="SQUARES CLAIMED" value={String(stats.totalSquaresClaimed)} />
            <StatBox label="COINS WAGERED" value={String(stats.totalWagered)} />
            <StatBox label="COINS WON" value={String(stats.wagersWon)} />
          </div>
        ) : (
          <p className="font-['VT323'] text-gray-500 text-xl text-center py-8">NO DATA AVAILABLE</p>
        )}
      </RetroCard>

      {/* Game History Tabs */}
      <RetroCard title="GAME HISTORY">
        <Tabs defaultValue="current" onValueChange={handleTabChange} className="pt-4">
          <TabsList className="w-full bg-black border-2 border-primary/30 rounded-none p-0 h-auto">
            <TabsTrigger
              value="current"
              className="flex-1 rounded-none py-3 font-['Press_Start_2P'] text-xs text-primary border-r-2 border-primary/30
                data-[state=active]:bg-primary data-[state=active]:text-black data-[state=active]:shadow-none"
            >
              ◄ CURRENT
            </TabsTrigger>
            <TabsTrigger
              value="past"
              className="flex-1 rounded-none py-3 font-['Press_Start_2P'] text-xs text-primary
                data-[state=active]:bg-primary data-[state=active]:text-black data-[state=active]:shadow-none"
            >
              PAST ►
            </TabsTrigger>
          </TabsList>

          {/* Current Games */}
          <TabsContent value="current" className="mt-4">
            {!loaded.current ? (
              <EmptyState message="CLICK TAB TO LOAD" />
            ) : currentLoading ? (
              <LoadingRow />
            ) : !currentGames?.length ? (
              <EmptyState message="NO ACTIVE GAMES" />
            ) : (
              <div className="space-y-3">
                {currentGames.map(game => (
                  <Link key={game.gameId} href={`/game/${game.gameId}`}>
                    <div className="border-2 border-primary/30 hover:border-primary p-4 cursor-pointer transition-colors group">
                      <div className="flex justify-between items-start gap-4">
                        <div>
                          <p className="font-['Press_Start_2P'] text-white text-xs group-hover:text-primary transition-colors mb-2">
                            {game.gameName}
                          </p>
                          <p className="font-['VT323'] text-gray-400 text-lg">
                            {game.gameType.toUpperCase()} · {game.squaresClaimed} SQUARES · {game.pricePerSquare} COINS/SQ
                          </p>
                        </div>
                        <div className="flex flex-col items-end gap-1 shrink-0">
                          {game.isHost && (
                            <span className="font-['Press_Start_2P'] text-xs px-2 py-1 bg-primary/20 text-primary border border-primary/50">
                              HOST
                            </span>
                          )}
                          <span className={`font-['Press_Start_2P'] text-xs px-2 py-1 ${
                            game.selectionPhaseActive
                              ? 'bg-yellow-900 text-yellow-400 border border-yellow-600'
                              : game.isOpen
                              ? 'bg-green-900 text-green-400 border border-green-600'
                              : 'bg-blue-900 text-blue-400 border border-blue-600'
                          }`}>
                            {game.selectionPhaseActive ? 'SELECTING' : game.isOpen ? 'OPEN' : 'IN PROGRESS'}
                          </span>
                        </div>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </TabsContent>

          {/* Past Games */}
          <TabsContent value="past" className="mt-4">
            {!loaded.past ? (
              <EmptyState message="CLICK TAB TO LOAD" />
            ) : pastLoading ? (
              <LoadingRow />
            ) : !pastGamesData?.games?.length ? (
              <EmptyState message="NO PAST GAMES" />
            ) : (
              <>
                <div className="space-y-3">
                  {pastGamesData.games.map(game => (
                    <div key={game.gameId} className="border-2 border-primary/30 p-4">
                      <div className="flex justify-between items-start gap-4 mb-3">
                        <div>
                          <p className="font-['Press_Start_2P'] text-white text-xs mb-1">{game.gameName}</p>
                          <p className="font-['VT323'] text-gray-400 text-lg">
                            {game.gameType.toUpperCase()} · {format(new Date(game.createdAt), 'MMM dd, yyyy')}
                          </p>
                        </div>
                        {game.periodsWon > 0 && (
                          <span className="font-['Press_Start_2P'] text-xs px-2 py-1 bg-primary text-black shrink-0">
                            {game.periodsWon}W
                          </span>
                        )}
                      </div>
                      <div className="grid grid-cols-3 gap-2 pt-3 border-t border-primary/20">
                        <div className="text-center">
                          <p className="font-['VT323'] text-gray-500 text-base">SQUARES</p>
                          <p className="font-['Press_Start_2P'] text-primary text-xs">{game.squaresClaimed}</p>
                        </div>
                        <div className="text-center">
                          <p className="font-['VT323'] text-gray-500 text-base">WAGERED</p>
                          <p className="font-['Press_Start_2P'] text-primary text-xs">{game.totalWagered}</p>
                        </div>
                        <div className="text-center">
                          <p className="font-['VT323'] text-gray-500 text-base">WON</p>
                          <p className={`font-['Press_Start_2P'] text-xs ${game.totalWon > 0 ? 'text-green-400' : 'text-gray-600'}`}>
                            {game.totalWon}
                          </p>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

                {totalPages > 1 && (
                  <div className="flex items-center justify-between mt-6 pt-4 border-t-2 border-primary/20">
                    <RetroButton
                      variant="outline"
                      size="sm"
                      onClick={() => setPage(p => Math.max(1, p - 1))}
                      disabled={page === 1}
                    >
                      ◄ PREV
                    </RetroButton>
                    <span className="font-['VT323'] text-gray-400 text-xl">
                      {page} / {totalPages}
                    </span>
                    <RetroButton
                      variant="outline"
                      size="sm"
                      onClick={() => setPage(p => p + 1)}
                      disabled={page >= totalPages}
                    >
                      NEXT ►
                    </RetroButton>
                  </div>
                )}
              </>
            )}
          </TabsContent>
        </Tabs>
      </RetroCard>
    </div>
  );
}
