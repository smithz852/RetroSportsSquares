import { useState } from "react";
import { Link, useLocation } from "wouter";
import { useAuth } from "@/hooks/use-auth";
import { usePlayerStats, useCurrentGames, usePastGames } from "@/hooks/use-dashboard";
import { RetroButton } from "@/components/RetroButton";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Loader2, Trophy, User, Settings, Shield } from "lucide-react";
import { format } from "date-fns";

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

function StatRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between items-center py-2 border-b border-primary/10 last:border-0">
      <span className="font-['VT323'] text-gray-400 text-xl tracking-wider">{label}</span>
      <span className="font-['Press_Start_2P'] text-primary text-sm text-shadow-retro">{value}</span>
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
    if ((key === 'current' || key === 'past') && !loaded[key]) {
      setLoaded(prev => ({ ...prev, [key]: true }));
    }
  };

  const formatWinRate = (rate: number) => {
    if (rate === 0) return ".000";
    return rate.toFixed(3).replace("0.", ".");
  };

  const totalPages = Math.ceil((pastGamesData?.totalCount ?? 0) / PAGE_SIZE);

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="border-b-2 border-primary/30 pb-4">
        <h1 className="text-xl md:text-2xl font-['Press_Start_2P'] text-primary text-shadow-retro mb-1">
          PLAYER DASHBOARD
        </h1>
        <p className="font-['VT323'] text-gray-400 text-xl tracking-widest">
          {user.displayName?.toUpperCase() ?? "UNKNOWN"}
        </p>
      </div>

      {/* Main layout: left column + right panel */}
      <div className="flex gap-3 min-h-[480px]">

        {/* ── Left column ── */}
        <div className="flex flex-col gap-3 w-36 shrink-0">

          {/* Avatar / profile placeholder */}
          <div className="border-4 border-primary box-shadow-retro aspect-square flex flex-col items-center justify-center bg-black gap-1">
            <User className="w-12 h-12 text-primary/40" />
            <span className="font-['Press_Start_2P'] text-primary/30 text-[8px] text-center leading-3">
              PROFILE
            </span>
          </div>

          {/* Badge + Periods Won */}
          <div className="border-4 border-primary box-shadow-retro flex-1 flex flex-col items-center justify-start gap-4 bg-black p-3">
            {/* Badge placeholder */}
            <div className="flex flex-col items-center gap-1 pt-1">
              <Shield className="w-10 h-10 text-primary/30" />
              <span className="font-['Press_Start_2P'] text-primary/30 text-[8px] text-center leading-3">
                RANK
              </span>
              <span className="font-['VT323'] text-gray-600 text-base">-- UNRANKED --</span>
            </div>

            {/* Divider */}
            <div className="w-full border-t-2 border-primary/20" />

            {/* Periods Won */}
            <div className="flex flex-col items-center gap-1">
              <span className="font-['VT323'] text-gray-400 text-lg tracking-wider text-center">PERIODS WON</span>
              <span className="font-['Press_Start_2P'] text-primary text-2xl text-shadow-retro">
                {statsLoading ? (
                  <Loader2 className="h-5 w-5 animate-spin" />
                ) : (
                  stats?.periodsWon ?? 0
                )}
              </span>
            </div>
          </div>

          {/* Settings */}
          <Link href="/settings">
            <div className="border-4 border-primary box-shadow-retro bg-black flex flex-col items-center justify-center gap-2 p-4 cursor-pointer hover:bg-primary/10 transition-colors group">
              <Settings className="w-8 h-8 text-primary group-hover:rotate-45 transition-transform duration-300" />
              <span className="font-['Press_Start_2P'] text-primary/60 text-[8px]">SETTINGS</span>
            </div>
          </Link>
        </div>

        {/* ── Right panel ── */}
        <div className="border-4 border-primary box-shadow-retro bg-black flex-1 flex flex-col">
          <Tabs defaultValue="stats" onValueChange={handleTabChange} className="flex flex-col h-full">

            {/* Tab buttons at the top of the panel */}
            <TabsList className="w-full bg-black border-b-4 border-primary rounded-none p-0 h-auto shrink-0">
              <TabsTrigger
                value="stats"
                className="flex-1 rounded-none py-3 font-['Press_Start_2P'] text-[10px] text-primary border-r-2 border-primary/40
                  data-[state=active]:bg-primary data-[state=active]:text-black data-[state=active]:shadow-none"
              >
                STATS
              </TabsTrigger>
              <TabsTrigger
                value="current"
                className="flex-1 rounded-none py-3 font-['Press_Start_2P'] text-[10px] text-primary border-r-2 border-primary/40
                  data-[state=active]:bg-primary data-[state=active]:text-black data-[state=active]:shadow-none"
              >
                CURRENT
              </TabsTrigger>
              <TabsTrigger
                value="past"
                className="flex-1 rounded-none py-3 font-['Press_Start_2P'] text-[10px] text-primary
                  data-[state=active]:bg-primary data-[state=active]:text-black data-[state=active]:shadow-none"
              >
                PAST
              </TabsTrigger>
            </TabsList>

            {/* Stats Tab */}
            <TabsContent value="stats" className="flex-1 p-4 mt-0">
              {statsLoading ? (
                <LoadingRow />
              ) : stats ? (
                <div className="space-y-1">
                  <StatRow label="WIN RATE" value={formatWinRate(stats.winRate)} />
                  <StatRow label="COINS WAGERED" value={String(stats.totalWagered)} />
                  <StatRow label="COINS WON" value={String(stats.wagersWon)} />
                  <StatRow label="SQUARES CLAIMED" value={String(stats.totalSquaresClaimed)} />
                </div>
              ) : (
                <p className="font-['VT323'] text-gray-500 text-xl text-center py-8">NO DATA AVAILABLE</p>
              )}
            </TabsContent>

            {/* Current Games Tab */}
            <TabsContent value="current" className="flex-1 p-4 mt-0 overflow-y-auto">
              {!loaded.current ? (
                <EmptyState message="LOADING..." />
              ) : currentLoading ? (
                <LoadingRow />
              ) : !currentGames?.length ? (
                <EmptyState message="NO ACTIVE GAMES" />
              ) : (
                <div className="space-y-3">
                  {currentGames.map(game => (
                    <Link key={game.gameId} href={`/game/${game.gameId}`}>
                      <div className="border-2 border-primary/30 hover:border-primary p-3 cursor-pointer transition-colors group">
                        <div className="flex justify-between items-start gap-3">
                          <div>
                            <p className="font-['Press_Start_2P'] text-white text-[10px] group-hover:text-primary transition-colors mb-2 leading-4">
                              {game.gameName}
                            </p>
                            <p className="font-['VT323'] text-gray-400 text-lg">
                              {game.gameType.toUpperCase()} · {game.squaresClaimed} SQ · {game.pricePerSquare}¢/SQ
                            </p>
                          </div>
                          <div className="flex flex-col items-end gap-1 shrink-0">
                            {game.isHost && (
                              <span className="font-['Press_Start_2P'] text-[8px] px-1.5 py-0.5 bg-primary/20 text-primary border border-primary/50">
                                HOST
                              </span>
                            )}
                            <span className={`font-['Press_Start_2P'] text-[8px] px-1.5 py-0.5 ${
                              game.selectionPhaseActive
                                ? 'bg-yellow-900 text-yellow-400 border border-yellow-600'
                                : game.isOpen
                                ? 'bg-green-900 text-green-400 border border-green-600'
                                : 'bg-blue-900 text-blue-400 border border-blue-600'
                            }`}>
                              {game.selectionPhaseActive ? 'SELECTING' : game.isOpen ? 'OPEN' : 'IN PROG'}
                            </span>
                          </div>
                        </div>
                      </div>
                    </Link>
                  ))}
                </div>
              )}
            </TabsContent>

            {/* Past Games Tab */}
            <TabsContent value="past" className="flex-1 p-4 mt-0 overflow-y-auto">
              {!loaded.past ? (
                <EmptyState message="LOADING..." />
              ) : pastLoading ? (
                <LoadingRow />
              ) : !pastGamesData?.games?.length ? (
                <EmptyState message="NO PAST GAMES" />
              ) : (
                <>
                  <div className="space-y-3">
                    {pastGamesData.games.map(game => (
                      <div key={game.gameId} className="border-2 border-primary/30 p-3">
                        <div className="flex justify-between items-start gap-3 mb-2">
                          <div>
                            <p className="font-['Press_Start_2P'] text-white text-[10px] mb-1 leading-4">{game.gameName}</p>
                            <p className="font-['VT323'] text-gray-400 text-lg">
                              {game.gameType.toUpperCase()} · {format(new Date(game.createdAt), 'MMM dd, yyyy')}
                            </p>
                          </div>
                          {game.periodsWon > 0 && (
                            <span className="font-['Press_Start_2P'] text-[10px] px-2 py-1 bg-primary text-black shrink-0">
                              {game.periodsWon}W
                            </span>
                          )}
                        </div>
                        <div className="grid grid-cols-3 gap-2 pt-2 border-t border-primary/20">
                          <div className="text-center">
                            <p className="font-['VT323'] text-gray-500 text-sm">SQUARES</p>
                            <p className="font-['Press_Start_2P'] text-primary text-[10px]">{game.squaresClaimed}</p>
                          </div>
                          <div className="text-center">
                            <p className="font-['VT323'] text-gray-500 text-sm">WAGERED</p>
                            <p className="font-['Press_Start_2P'] text-primary text-[10px]">{game.totalWagered}</p>
                          </div>
                          <div className="text-center">
                            <p className="font-['VT323'] text-gray-500 text-sm">WON</p>
                            <p className={`font-['Press_Start_2P'] text-[10px] ${game.totalWon > 0 ? 'text-green-400' : 'text-gray-600'}`}>
                              {game.totalWon}
                            </p>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>

                  {totalPages > 1 && (
                    <div className="flex items-center justify-between mt-4 pt-4 border-t-2 border-primary/20">
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
        </div>
      </div>
    </div>
  );
}
