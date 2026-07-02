import { useState } from "react";
import { useLocation } from "wouter";
import {
  useAdminSummary,
  useAdminCurrentGames,
  useAdminPastGames,
  useAdminPlayerStats,
  useAdminUsers,
  useRefreshAdminData,
} from "@/hooks/use-admin-dashboard";
import { RetroButton } from "@/components/RetroButton";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Loader2, RefreshCw, Users, Gamepad2, Trophy, Coins } from "lucide-react";
import { format } from "date-fns";

const PAGE_SIZE = 10;

function LoadingRow() {
  return (
    <div className="flex items-center justify-center gap-3 py-10">
      <Loader2 className="h-6 w-6 text-primary animate-spin" />
      <span className="font-['Press_Start_2P'] text-primary text-xs animate-pulse">LOADING...</span>
    </div>
  );
}

function ErrorState({ message }: { message: string }) {
  return (
    <div className="text-center py-12 border-2 border-dashed border-primary/20">
      <p className="font-['Press_Start_2P'] text-primary text-xs">{message.toUpperCase()}</p>
    </div>
  );
}

function EmptyState({ message }: { message: string }) {
  return (
    <div className="text-center py-12 border-2 border-dashed border-primary/20">
      <p className="font-['Press_Start_2P'] text-gray-600 text-xs">{message}</p>
    </div>
  );
}

function StatTile({ icon: Icon, label, value, loading }: {
  icon: typeof Users;
  label: string;
  value: string | number;
  loading: boolean;
}) {
  return (
    <div className="border-4 border-primary box-shadow-retro bg-black p-4 flex flex-col items-center gap-2">
      <Icon className="w-6 h-6 text-primary/50" />
      <span className="font-['VT323'] text-gray-400 text-lg tracking-wider text-center">{label}</span>
      <span className="font-['Press_Start_2P'] text-primary text-lg text-shadow-retro">
        {loading ? <Loader2 className="h-5 w-5 animate-spin" /> : value}
      </span>
    </div>
  );
}

const headCell = "font-['Press_Start_2P'] text-primary text-[9px] whitespace-nowrap";
const bodyCell = "font-['VT323'] text-gray-300 text-lg whitespace-nowrap";

function StatusBadge({ isOpen, selectionPhaseActive }: { isOpen: boolean; selectionPhaseActive: boolean }) {
  return (
    <span className={`font-['Press_Start_2P'] text-[8px] px-1.5 py-0.5 ${
      selectionPhaseActive
        ? 'bg-yellow-900 text-yellow-400 border border-yellow-600'
        : isOpen
        ? 'bg-green-900 text-green-400 border border-green-600'
        : 'bg-blue-900 text-blue-400 border border-blue-600'
    }`}>
      {selectionPhaseActive ? 'SELECTING' : isOpen ? 'OPEN' : 'IN PROG'}
    </span>
  );
}

export default function AdminDashboard() {
  const [, setLocation] = useLocation();
  const [pastPage, setPastPage] = useState(1);

  // Open the board in admin spectator mode (view without joining)
  const spectateGame = (gameId: string) => setLocation(`/game/${gameId}?spectate=1`);

  const { data: summary, isLoading: summaryLoading, error: summaryError } = useAdminSummary();
  const { data: currentGames, isLoading: currentLoading, error: currentError } = useAdminCurrentGames();
  const { data: pastGamesData, isLoading: pastLoading, error: pastError } = useAdminPastGames(pastPage, PAGE_SIZE);
  const { data: playerStats, isLoading: playersLoading, error: playersError } = useAdminPlayerStats();
  const { data: users, isLoading: usersLoading, error: usersError } = useAdminUsers();
  const refreshAll = useRefreshAdminData();

  const totalPastPages = Math.ceil((pastGamesData?.totalCount ?? 0) / PAGE_SIZE);

  const formatWinRate = (rate: number) => {
    if (rate === 0) return ".000";
    return rate.toFixed(3).replace("0.", ".");
  };

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="border-b-2 border-primary/30 pb-4 flex justify-between items-end">
        <div>
          <h1 className="text-xl md:text-2xl font-['Press_Start_2P'] text-primary text-shadow-retro mb-1">
            ADMIN DASHBOARD
          </h1>
          <p className="font-['VT323'] text-gray-400 text-xl tracking-widest">
            PLATFORM OVERVIEW
          </p>
        </div>
        <RetroButton variant="outline" size="sm" onClick={refreshAll}>
          <RefreshCw className="w-4 h-4 mr-1" />
          REFRESH
        </RetroButton>
      </div>

      {/* Summary tiles */}
      {summaryError ? (
        <ErrorState message={summaryError.message} />
      ) : (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <StatTile icon={Users} label="TOTAL USERS" value={summary?.totalUsers ?? 0} loading={summaryLoading} />
          <StatTile icon={Gamepad2} label="ACTIVE GAMES" value={summary?.activeGames ?? 0} loading={summaryLoading} />
          <StatTile icon={Trophy} label="COMPLETED" value={summary?.completedGames ?? 0} loading={summaryLoading} />
          <StatTile icon={Coins} label="COINS WAGERED" value={(summary?.totalCoinsWagered ?? 0).toFixed(2)} loading={summaryLoading} />
        </div>
      )}

      {/* Tables panel */}
      <div className="border-4 border-primary box-shadow-retro bg-black">
        <Tabs defaultValue="current" className="flex flex-col">
          <TabsList className="w-full bg-black border-b-4 border-primary rounded-none p-0 h-auto shrink-0">
            {[
              { value: "current", label: "CURRENT" },
              { value: "past", label: "PAST" },
              { value: "players", label: "PLAYERS" },
              { value: "users", label: "USERS" },
            ].map((tab, i, arr) => (
              <TabsTrigger
                key={tab.value}
                value={tab.value}
                className={`flex-1 rounded-none py-3 font-['Press_Start_2P'] text-[10px] text-primary
                  ${i < arr.length - 1 ? 'border-r-2 border-primary/40' : ''}
                  data-[state=active]:bg-primary data-[state=active]:text-black data-[state=active]:shadow-none`}
              >
                {tab.label}
              </TabsTrigger>
            ))}
          </TabsList>

          {/* Current games */}
          <TabsContent value="current" className="p-4 mt-0 overflow-x-auto">
            {currentLoading ? (
              <LoadingRow />
            ) : currentError ? (
              <ErrorState message={currentError.message} />
            ) : !currentGames?.length ? (
              <EmptyState message="NO ACTIVE GAMES" />
            ) : (
              <Table>
                <TableHeader>
                  <TableRow className="border-primary/30 hover:bg-transparent">
                    <TableHead className={headCell}>NAME</TableHead>
                    <TableHead className={headCell}>HOST</TableHead>
                    <TableHead className={headCell}>SPORT / LEAGUE</TableHead>
                    <TableHead className={headCell}>MATCHUP</TableHead>
                    <TableHead className={headCell}>PLAYERS</TableHead>
                    <TableHead className={headCell}>PRICE/SQ</TableHead>
                    <TableHead className={headCell}>SQ CLAIMED</TableHead>
                    <TableHead className={headCell}>STATUS</TableHead>
                    <TableHead className={headCell}>CREATED</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {currentGames.map(game => (
                    <TableRow
                      key={game.gameId}
                      onClick={() => spectateGame(game.gameId)}
                      className="border-primary/20 hover:bg-primary/5 cursor-pointer"
                    >
                      <TableCell className={`${bodyCell} text-white`}>{game.gameName}</TableCell>
                      <TableCell className={bodyCell}>{game.hostDisplayName ?? '—'}</TableCell>
                      <TableCell className={bodyCell}>{game.gameType.toUpperCase()} / {game.league}</TableCell>
                      <TableCell className={bodyCell}>{game.matchup}</TableCell>
                      <TableCell className={bodyCell}>{game.playersJoined}/{game.maxPlayers}</TableCell>
                      <TableCell className={bodyCell}>{game.pricePerSquare.toFixed(2)}</TableCell>
                      <TableCell className={bodyCell}>{game.squaresClaimed}</TableCell>
                      <TableCell>
                        <StatusBadge isOpen={game.isOpen} selectionPhaseActive={game.selectionPhaseActive} />
                      </TableCell>
                      <TableCell className={bodyCell}>{format(new Date(game.createdAt), 'MMM dd, yyyy')}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </TabsContent>

          {/* Past games */}
          <TabsContent value="past" className="p-4 mt-0 overflow-x-auto">
            {pastLoading ? (
              <LoadingRow />
            ) : pastError ? (
              <ErrorState message={pastError.message} />
            ) : !pastGamesData?.games?.length ? (
              <EmptyState message="NO PAST GAMES" />
            ) : (
              <>
                <Table>
                  <TableHeader>
                    <TableRow className="border-primary/30 hover:bg-transparent">
                      <TableHead className={headCell}>NAME</TableHead>
                      <TableHead className={headCell}>HOST</TableHead>
                      <TableHead className={headCell}>SPORT / LEAGUE</TableHead>
                      <TableHead className={headCell}>MATCHUP</TableHead>
                      <TableHead className={headCell}>PLAYERS</TableHead>
                      <TableHead className={headCell}>POT</TableHead>
                      <TableHead className={headCell}>WINNERS</TableHead>
                      <TableHead className={headCell}>CREATED</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {pastGamesData.games.map(game => (
                      <TableRow
                        key={game.gameId}
                        onClick={() => spectateGame(game.gameId)}
                        className="border-primary/20 hover:bg-primary/5 cursor-pointer"
                      >
                        <TableCell className={`${bodyCell} text-white`}>{game.gameName}</TableCell>
                        <TableCell className={bodyCell}>{game.hostDisplayName ?? '—'}</TableCell>
                        <TableCell className={bodyCell}>{game.gameType.toUpperCase()} / {game.league}</TableCell>
                        <TableCell className={bodyCell}>{game.matchup}</TableCell>
                        <TableCell className={bodyCell}>{game.playersJoined}</TableCell>
                        <TableCell className={`${bodyCell} text-green-400`}>{game.totalPot.toFixed(2)}</TableCell>
                        <TableCell className={bodyCell}>
                          {game.periodWinners.map(pw => (
                            <div key={pw.period}>
                              P{pw.period}: {pw.winnerDisplayName ?? '—'}
                            </div>
                          ))}
                        </TableCell>
                        <TableCell className={bodyCell}>{format(new Date(game.createdAt), 'MMM dd, yyyy')}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>

                {totalPastPages > 1 && (
                  <div className="flex items-center justify-between mt-4 pt-4 border-t-2 border-primary/20">
                    <RetroButton
                      variant="outline"
                      size="sm"
                      onClick={() => setPastPage(p => Math.max(1, p - 1))}
                      disabled={pastPage === 1}
                    >
                      ◄ PREV
                    </RetroButton>
                    <span className="font-['VT323'] text-gray-400 text-xl">
                      {pastPage} / {totalPastPages}
                    </span>
                    <RetroButton
                      variant="outline"
                      size="sm"
                      onClick={() => setPastPage(p => p + 1)}
                      disabled={pastPage >= totalPastPages}
                    >
                      NEXT ►
                    </RetroButton>
                  </div>
                )}
              </>
            )}
          </TabsContent>

          {/* Player stats */}
          <TabsContent value="players" className="p-4 mt-0 overflow-x-auto">
            {playersLoading ? (
              <LoadingRow />
            ) : playersError ? (
              <ErrorState message={playersError.message} />
            ) : !playerStats?.length ? (
              <EmptyState message="NO PLAYER DATA" />
            ) : (
              <Table>
                <TableHeader>
                  <TableRow className="border-primary/30 hover:bg-transparent">
                    <TableHead className={headCell}>PLAYER</TableHead>
                    <TableHead className={headCell}>GAMER TAG</TableHead>
                    <TableHead className={headCell}>GAMES</TableHead>
                    <TableHead className={headCell}>SQUARES</TableHead>
                    <TableHead className={headCell}>PERIODS WON</TableHead>
                    <TableHead className={headCell}>WAGERED</TableHead>
                    <TableHead className={headCell}>WON</TableHead>
                    <TableHead className={headCell}>WIN RATE</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {playerStats.map(player => (
                    <TableRow key={player.userId} className="border-primary/20 hover:bg-primary/5">
                      <TableCell className={`${bodyCell} text-white`}>{player.displayName ?? '—'}</TableCell>
                      <TableCell className={bodyCell}>{player.gamerTag ?? '—'}</TableCell>
                      <TableCell className={bodyCell}>{player.gamesPlayed}</TableCell>
                      <TableCell className={bodyCell}>{player.totalSquaresClaimed}</TableCell>
                      <TableCell className={bodyCell}>{player.periodsWon}</TableCell>
                      <TableCell className={bodyCell}>{player.totalWagered.toFixed(2)}</TableCell>
                      <TableCell className={`${bodyCell} ${player.wagersWon > 0 ? 'text-green-400' : ''}`}>
                        {player.wagersWon.toFixed(2)}
                      </TableCell>
                      <TableCell className={bodyCell}>{formatWinRate(player.winRate)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </TabsContent>

          {/* Users */}
          <TabsContent value="users" className="p-4 mt-0 overflow-x-auto">
            {usersLoading ? (
              <LoadingRow />
            ) : usersError ? (
              <ErrorState message={usersError.message} />
            ) : !users?.length ? (
              <EmptyState message="NO USERS" />
            ) : (
              <Table>
                <TableHeader>
                  <TableRow className="border-primary/30 hover:bg-transparent">
                    <TableHead className={headCell}>EMAIL</TableHead>
                    <TableHead className={headCell}>DISPLAY NAME</TableHead>
                    <TableHead className={headCell}>GAMER TAG</TableHead>
                    <TableHead className={headCell}>JOINED</TableHead>
                    <TableHead className={headCell}>GAMES</TableHead>
                    <TableHead className={headCell}>ROLE</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {users.map(u => (
                    <TableRow key={u.id} className="border-primary/20 hover:bg-primary/5">
                      <TableCell className={`${bodyCell} text-white`}>{u.email ?? '—'}</TableCell>
                      <TableCell className={bodyCell}>{u.displayName ?? '—'}</TableCell>
                      <TableCell className={bodyCell}>{u.gamerTag ?? '—'}</TableCell>
                      <TableCell className={bodyCell}>{format(new Date(u.createdAt), 'MMM dd, yyyy')}</TableCell>
                      <TableCell className={bodyCell}>{u.gamesPlayed}</TableCell>
                      <TableCell>
                        {u.isAdmin && (
                          <span className="font-['Press_Start_2P'] text-[8px] px-1.5 py-0.5 bg-primary/20 text-primary border border-primary/50">
                            ADMIN
                          </span>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}
