import { useState, useEffect, useMemo, useRef, useCallback } from "react";
import { useParams, Link, useLocation } from "wouter";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { motion } from "framer-motion";
import { useToast } from "@/hooks/use-toast";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Coins } from "lucide-react";
import { Scoreboard } from "@/components/Scoreboard";
import { useAuth } from "@/hooks/use-auth";
import { GetGameScoreData, getSquareGameById, useStartGame, useDeleteGame, useGetTurnStatus, useBeginSelections, useSkipPlayer } from "@/hooks/use-games";
import { usePostSquareSelection, useGetBoardSquares, useGetOutsideSquares, useJoinGame, useLeaveGame } from "@/hooks/use-gameplay";
import { useQueryClient } from "@tanstack/react-query";
import { getCurrentGamePeriodIndex } from "@/components/Scoreboard";
import { useGameHub } from "@/hooks/use-game-hub";

export default function GameBoard() {
  const { user, isLoading: authLoading } = useAuth();
  const params = useParams();
  const id = params.id as string;
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [, setLocation] = useLocation();

  const [gameStarted, setGameStarted] = useState(false);

  const { data: game, isLoading: gameLoading, error } = getSquareGameById(id);
  const { data: boardSquares } = useGetBoardSquares(id);
  const { data: outsideSquares } = useGetOutsideSquares(id);

  const [topNumbers, setTopNumbers] = useState<(number | null)[]>(
    Array(10).fill(null),
  );
  const [leftNumbers, setLeftNumbers] = useState<(number | null)[]>(
    Array(10).fill(null),
  );
  const [selections, setSelections] = useState<Record<string, string>>({}); // { [squareGuid]: playerName }
  // console.log(selections);
  
  const [homeTeam, setHomeTeam] = useState("");
  const [awayTeam, setAwayTeam] = useState("");
  const { mutate, isPending } = usePostSquareSelection(id);
  const { mutate: joinGame } = useJoinGame();
  const { mutate: leaveGame, isPending: isLeavePending } = useLeaveGame();
  const { mutate: deleteGame } = useDeleteGame();
  const { mutate: startGame } = useStartGame(id);
  const { mutate: beginSelections, isPending: isBeginPending } = useBeginSelections(id);
  const { mutate: skipPlayer, isPending: isSkipPending } = useSkipPlayer(id);
  const { data: scoreData, isLoading } = GetGameScoreData(id);

  useGameHub(id);

  const isTurnBased = game?.isTurnBased ?? false;
  const { data: turnStatus } = useGetTurnStatus(id, !gameStarted);
  const selectionPhaseActive = turnStatus?.selectionPhaseActive ?? game?.selectionPhaseActive ?? false;

  const isMyTurn = isTurnBased
    ? turnStatus?.currentTurnUserId === user?.id
    : true;

  const [countdown, setCountdown] = useState<number | null>(null);
  const autoSubmitRef = useRef(false);

  const squareByPosition = useMemo(() => {
    if (!gameStarted || !boardSquares) return {};
    return Object.fromEntries(boardSquares.map(sq => [`${sq.rowIndex}-${sq.colIndex}`, sq]));
  }, [gameStarted, boardSquares]);

  const hasSubmittedSelections = useMemo(() => {
    if (!boardSquares || !user) return false;
    const mySquareCount = boardSquares.filter(s => s.displayName === user.displayName).length;
    const limit = game?.squareSelectionLimit;
    if (limit && limit > 0) return mySquareCount >= limit;
    return mySquareCount > 0;
  }, [boardSquares, user, game?.squareSelectionLimit]);

  const [activePlayer, setActivePlayer] = useState(() => {
    return localStorage.getItem("sports_squares_player") || "";
  });
  const [isHost, setIsHost] = useState(false);

  // Odds Board State
  const [multiplier, setMultiplier] = useState(0);
  const [tempMultiplier, setTempMultiplier] = useState(0);
  const [periodWinners, setPeriodWinners] = useState<Record<number, string | null>>({});
  const [currentLeader, setCurrentLeader] = useState<string | null>(null);
  const [winningRow, setWinningRow] = useState<number | null>(null);
  const [winningCol, setWinningCol] = useState<number | null>(null);

const currentPeriod = getCurrentGamePeriodIndex(scoreData?.status, scoreData?.sportType);

useEffect(() => {
  if (scoreData?.periodWinners) {
    setPeriodWinners(scoreData.periodWinners);
  }
}, [scoreData]);

// Track current leader based on live score
useEffect(() => {
  if (scoreData && gameStarted && scoreData.currentHomeScore != null && scoreData.currentAwayScore != null) {
    const homeDigit = scoreData.currentHomeScore % 10;
    const awayDigit = scoreData.currentAwayScore % 10;
    
    // Find the leading square based on current score digits ******
    const leadingSquare = squareByPosition[`${leftNumbers.indexOf(awayDigit)}-${topNumbers.indexOf(homeDigit)}`];
    setCurrentLeader(leadingSquare?.displayName || null);

    const rowIndex = leftNumbers.indexOf(awayDigit);
    const colIndex = topNumbers.indexOf(homeDigit);
    
    setWinningRow(rowIndex !== -1 ? rowIndex : null);
    setWinningCol(colIndex !== -1 ? colIndex : null);
  } else {
    setCurrentLeader(null);
    setWinningRow(null);
    setWinningCol(null);
  }
}, [scoreData?.currentHomeScore, scoreData?.currentAwayScore, gameStarted, squareByPosition, leftNumbers, topNumbers]);

  // Register the current user as a game player when they open the board
  useEffect(() => {
    if (user && id) {
      joinGame(id, {
        onSuccess: (data) => setIsHost(data.isHost),
      });
    }
  }, [user, id]);

  // Update state when game data loads
  useEffect(() => {
    if (game) {
      setHomeTeam(game.homeTeam || "");
      setAwayTeam(game.awayTeam || "");
      setMultiplier(game.pricePerSquare || 0);
      setTempMultiplier(game.pricePerSquare || 0);

      if (user) {
        setActivePlayer(user.displayName);
        toast({
          title: "PLAYER SET",
          description: `Active player: ${user.displayName || "NONE"}`,
        });
      }
    }
  }, [game]);

  // Load outside squares from DB
  useEffect(() => {
  const validOutside =
    outsideSquares &&
    Array.isArray(outsideSquares.topNumbers) &&
    outsideSquares.topNumbers.length === 10 &&
    Array.isArray(outsideSquares.leftNumbers) &&
    outsideSquares.leftNumbers.length === 10;

  if (!validOutside) return;

  setTopNumbers(outsideSquares.topNumbers);
  setLeftNumbers(outsideSquares.leftNumbers);
  setGameStarted(true);
}, [outsideSquares]);

  // Countdown timer — starts/resets when it becomes this player's turn
  useEffect(() => {
    if (!isTurnBased || !turnStatus?.selectionPhaseActive || !isMyTurn) {
      setCountdown(null);
      autoSubmitRef.current = false;
      return;
    }
    const timeout = turnStatus.turnTimeoutSeconds;
    if (timeout <= 0) { setCountdown(null); return; }

    const elapsed = turnStatus.turnStartedAt
      ? Math.floor((Date.now() - new Date(turnStatus.turnStartedAt).getTime()) / 1000)
      : 0;
    const remaining = Math.max(timeout - elapsed, 0);
    setCountdown(remaining);
    autoSubmitRef.current = false;

    const interval = setInterval(() => {
      setCountdown(prev => {
        if (prev === null || prev <= 1) {
          clearInterval(interval);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(interval);
  }, [turnStatus?.currentTurnUserId, turnStatus?.turnStartedAt, isMyTurn]);

  // Auto-submit when countdown hits 0
  useEffect(() => {
    if (countdown !== 0 || !isMyTurn || autoSubmitRef.current) return;
    autoSubmitRef.current = true;
    const selectionArray = Object.keys(selections).map(squareId => ({ squareId }));
    if (selectionArray.length === 0) {
      // No selections — just skip (submit empty to advance turn)
      mutate({ selections: [] }, {
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['boardSquares', id] }),
      });
    } else {
      mutate({ selections: selectionArray }, {
        onSuccess: () => {
          setSelections({});
          queryClient.invalidateQueries({ queryKey: ['boardSquares', id] });
        },
      });
    }
  }, [countdown]);

  // Redirect if not authenticated
  if (authLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-black">
        <h2 className="text-primary font-pixel animate-pulse">
          AUTHENTICATING...
        </h2>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-black gap-6">
        <h2 className="text-red-500 font-pixel text-center">
          ACCESS DENIED - LOGIN REQUIRED
        </h2>
        <Link href="/login">
          <Button className="bg-red-600 text-black font-pixel py-4 px-8 rounded-none hover:bg-red-500 active:translate-y-1 transition-all uppercase">
            LOGIN
          </Button>
        </Link>
      </div>
    );
  }

  // Validate game ID (GUID format)
  const guidRegex =
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
  if (!id || !guidRegex.test(id)) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-black">
        <h2 className="text-red-500 font-pixel">INVALID GAME ID</h2>
      </div>
    );
  }

  const handleSetMultiplier = () => {
    if (tempMultiplier >= 0) {
      setMultiplier(tempMultiplier);
      toast({
        title: "MULTIPLIER SET",
        description: `Wager per square: ${tempMultiplier} coins`,
      });
    }
  };

  

  const handleStartGame = () => startGame();

  const handleDeleteGame = () => {
    if (!confirm("Delete this game? This cannot be undone.")) return;
    deleteGame(id, {
      onSuccess: () => setLocation("/"),
      onError: () => toast({
        title: "ERROR",
        description: "Failed to delete game.",
        variant: "destructive",
        className: "bg-black border-2 border-red-900 text-red-500 font-['VT323']",
      }),
    });
  };


  const handleLeaveGame = () => {
    if (!confirm("Leave this game?")) return;
    leaveGame(id, {
      onSuccess: () => setLocation("/"),
      onError: (err) => toast({
        title: "ERROR",
        description: err.message || "Failed to leave game.",
        variant: "destructive",
        className: "bg-black border-2 border-red-900 text-red-500 font-['VT323']",
      }),
    });
  };

  const handleSubmit = () => {
    const selectionArray = Object.keys(selections).map((key) => ({
      squareId: key
    }));

    mutate(
      {
        selections: selectionArray,
      },
      {
        onSuccess: (response) => {
          // Keep selections visible instead of clearing
          toast({
            title: "SQUARES SAVED",
            description: `${response.selections.length} squares claimed!`,
            className:
              "bg-black border-2 border-primary text-primary font-['VT323']",
          });
          queryClient.invalidateQueries({ queryKey: ['boardSquares', id] });
        },
        onError: (error: any) => {
          const message = error instanceof Error ? error.message : "FAILED TO SAVE SELECTIONS.";
          const unavailableSquares = error?.details;
          
          toast({
            title: "ERROR",
            description: message,
            variant: "destructive",
            className:
              "bg-black border-2 border-red-900 text-red-500 font-['VT323']",
          });
            queryClient.invalidateQueries({ queryKey: ['boardSquares', id] });
        },
      },
    );
    setSelections({});
  };

  

  const handleSquareClick = (squareId: string) => {
    if (gameStarted) return;

    if (isTurnBased && selectionPhaseActive && !isMyTurn) {
      toast({
        title: "NOT YOUR TURN",
        description: "Wait for your turn to select squares.",
        variant: "destructive",
      });
      return;
    }

    const savedSquare = boardSquares?.find(s => s.id === squareId);
    if (savedSquare?.displayName) {
      toast({
        title: "SQUARE TAKEN",
        description: `This square belongs to ${savedSquare.displayName}`,
        variant: "destructive",
      });
      return;
    }

    const playerName = user?.displayName || activePlayer;

    if (selections[squareId]) {
      const { [squareId]: _, ...rest } = selections;
      setSelections(rest);
    } else {
      setSelections({ ...selections, [squareId]: playerName });
    }
  };

  if (gameLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-black">
        <h2 className="text-primary font-pixel animate-pulse">
          LOADING GAME...
        </h2>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-black">
        <h2 className="text-red-500 font-pixel">
          ERROR: {(error as Error).message}
        </h2>
      </div>
    );
  }

  // Calculate odds data — combine saved and local unsaved selections
  const playerStats = [
    ...(boardSquares?.map(s => s.displayName).filter(Boolean) as string[] ?? []),
    ...Object.values(selections),
  ].reduce((acc, name) => {
    acc[name] = (acc[name] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  return (
    <div className="flex flex-col items-center p-4 max-w-[1400px] mx-auto w-full">
      <Scoreboard
        isVisible={gameStarted}
        gameName={(game as any)?.name}
        gameStartTime={game?.startTime}
        scoreData={scoreData}
        isLoading={isLoading}
        currentPeriod={currentPeriod}
        currentLeader={currentLeader}
        periodWinners={periodWinners}
      />

      <div className="flex flex-col items-center gap-8 w-full">
        <div className="flex flex-col items-center gap-8 w-full">
          <div className="flex items-center gap-4 w-full max-w-xl">
            {!gameStarted ? (
              <>
                  {isHost && isTurnBased && !selectionPhaseActive && (
                    <Button
                      onClick={() => beginSelections()}
                      disabled={isBeginPending}
                      className="w-full bg-red-600 text-black font-pixel text-xl py-8 rounded-none border-b-8 border-red-900 active:border-b-0 active:translate-y-2 transition-all hover:bg-red-500 animate-pulse"
                    >
                      BEGIN SELECTIONS
                    </Button>
                  )}

                  {isHost && (
                    <Button
                      onClick={handleStartGame}
                      className="w-full bg-red-600 text-black font-pixel text-xl py-8 rounded-none border-b-8 border-red-900 active:border-b-0 active:translate-y-2 transition-all hover:bg-red-500 animate-pulse"
                    >
                      INSERT COIN / START GAME
                    </Button>
                  )}

                  {isHost && isTurnBased && selectionPhaseActive && (
                    <Button
                      onClick={() => skipPlayer()}
                      disabled={isSkipPending}
                      className="bg-red-900 text-red-400 font-pixel text-xl py-8 rounded-none border-b-8 border-red-950 active:border-b-0 active:translate-y-2 transition-all hover:bg-red-800"
                    >
                      SKIP
                    </Button>
                  )}

                  {!hasSubmittedSelections && (!isTurnBased || selectionPhaseActive) && isMyTurn && (
                    <Button
                      onClick={handleSubmit}
                      disabled={isPending}
                      className="bg-red-600 text-black font-pixel text-xl py-8 rounded-none border-b-8 border-red-900 active:border-b-0 active:translate-y-2 transition-all hover:bg-red-500 animate-pulse"
                    >
                      {countdown !== null ? `SUBMIT (${countdown}s)` : "Submit"}
                    </Button>
                  )}

              </>
            ) : (
              <div className="w-full bg-red-900/20 border-2 border-red-600 p-4 font-pixel text-red-500 animate-pulse text-center uppercase">
                {(game as any)?.name} - IN PROGRESS
              </div>
            )}
          </div>

          {/* Game Board with Team Labels */}
          <div className="flex items-center gap-4">
            {/* Away Team Label (Rotated) */}
            <div className="flex items-center justify-center">
              <span className="-rotate-90 text-red-600 font-pixel text-sm whitespace-nowrap">
                {awayTeam}
              </span>
            </div>

            {/* Grid Container */}
            <div className="flex flex-col">
              {/* Home Team Label */}
              <div className="text-center mb-7 pl-10 md:pl-14">
                <span className="text-red-600 font-pixel text-sm">
                  {homeTeam}
                </span>
              </div>

              {/* Game Grid */}
              <div className="inline-grid grid-cols-11 border-4 border-red-900 bg-black p-1 shadow-[0_0_30px_rgba(255,0,0,0.2)]">
                <div
                  onClick={
                    isHost && !gameStarted ? handleDeleteGame
                    : !isHost && !gameStarted ? handleLeaveGame
                    : undefined
                  }
                  className={`w-10 h-10 md:w-14 md:h-14 border-2 border-red-900 flex items-center justify-center transition-colors ${
                    (isHost && !gameStarted) || (!isHost && !gameStarted)
                      ? "bg-red-600 cursor-pointer animate-[pulse_2s_infinite] hover:bg-red-500"
                      : "bg-red-900/20 cursor-default"
                  }`}
                >
                  <span className="text-black font-pixel text-[8px] md:text-[10px]">
                    {isHost && !gameStarted ? "DEL" : !isHost && !gameStarted ? "EXIT" : ""}
                  </span>
                </div>

                {topNumbers.map((num, i) => (
                  <div
                    key={`top-${i}`}
                    className="w-10 h-10 md:w-14 md:h-14 bg-red-600 border-2 border-red-900 flex items-center justify-center font-pixel text-black text-xl"
                  >
                    {num !== null ? num : "?"}
                  </div>
                ))}

                {Array.from({ length: 10 }).map((_, rowIndex) => (
                  <div key={`row-${rowIndex}`} className={`contents ${rowIndex === winningRow ? 'winning-row' : ''}`}>
                    <div className="w-10 h-10 md:w-14 md:h-14 bg-red-600 border-2 border-red-900 flex items-center justify-center font-pixel text-black text-xl">
                      {leftNumbers[rowIndex] !== null
                        ? leftNumbers[rowIndex]
                        : "?"}
                    </div>

                    {Array.from({ length: 10 }).map((_, colIndex) => {
                      const boardSquare = boardSquares?.find(s => s.rowIndex === rowIndex && s.colIndex === colIndex);
                      const squareId = boardSquare?.id;
                      const localSelection = squareId ? selections[squareId] : undefined;
                      const savedSquare = squareId ? boardSquares?.find(s => s.id === squareId) : undefined;
                      const displayName = savedSquare?.displayName || localSelection || "OPEN";
                      const isSelected = !!savedSquare?.displayName || !!localSelection;
                      const isWinningSquare = rowIndex === winningRow && colIndex === winningCol;

                      return (
                        <div
                          key={`${rowIndex}-${colIndex}`}
                          onClick={() => squareId && handleSquareClick(squareId)}
                          className={`w-10 h-10 md:w-14 md:h-14 border-2 border-red-900/30 flex flex-col items-center justify-center cursor-pointer transition-all ${
                            isSelected ? "bg-red-600/20" : "hover:bg-red-900/10"
                          } ${colIndex === winningCol ? 'winning-column' : ''} ${rowIndex === winningRow ? 'winning-row' : ''} ${isWinningSquare ? 'winning-square' : ''}`}
                        >
                          <span
                            className={`font-pixel text-[6px] md:text-[8px] text-center px-1 leading-tight ${isSelected ? "text-red-500" : "text-red-900/40"}`}
                          >
                            {displayName}
                          </span>
                        </div>
                      );
                    })}
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* Panels row — sits below the board */}
        <div className="flex flex-wrap gap-8 w-full justify-center">

          {/* Players / Turn Order Panel */}
          {!gameStarted && turnStatus && (
            <div className="flex-1 min-w-[280px] max-w-md">
              <Card className="bg-black border-4 border-red-900 rounded-none shadow-[0_0_20px_rgba(255,0,0,0.1)]">
                <CardHeader className="border-b-2 border-red-900 p-4">
                  <CardTitle className="text-xl text-red-600 font-pixel text-center uppercase tracking-tighter">
                    {selectionPhaseActive ? "TURN ORDER" : "PLAYERS"}
                  </CardTitle>
                </CardHeader>
                <CardContent className="p-4 space-y-2">
                  {turnStatus.players.length === 0 ? (
                    <p className="text-red-900/60 font-pixel text-[8px] text-center uppercase pt-2">
                      Waiting for players to join...
                    </p>
                  ) : selectionPhaseActive ? (
                    turnStatus.players.map((p) => {
                      const isCurrent = p.userId === turnStatus.currentTurnUserId;
                      return (
                        <motion.div
                          key={p.userId}
                          initial={{ opacity: 0, x: 10 }}
                          animate={{ opacity: 1, x: 0 }}
                          className={`flex items-center gap-3 p-2 border ${
                            isCurrent
                              ? "border-red-500 bg-red-900/20 animate-pulse"
                              : p.hasHadTurn
                              ? "border-red-900/20 opacity-40"
                              : "border-red-900/40"
                          }`}
                        >
                          <span className="font-pixel text-[8px] text-red-900/60 w-4">{p.turnOrder}</span>
                          <span className={`font-pixel text-[10px] flex-1 truncate ${isCurrent ? "text-red-400" : "text-red-700"}`}>
                            {p.displayName}
                            {p.isHost && <span className="text-red-900/60"> [HOST]</span>}
                            {p.hasHadTurn && " ✓"}
                          </span>
                          {isCurrent && countdown !== null && countdown > 0 && (
                            <span className="font-pixel text-[8px] text-red-500">{countdown}s</span>
                          )}
                        </motion.div>
                      );
                    })
                  ) : (
                    <>
                      {turnStatus.players.map((p) => (
                        <div
                          key={p.userId}
                          className={`flex items-center gap-3 p-2 border ${
                            p.hasHadTurn ? "border-red-900/20 opacity-40" : "border-red-900/40"
                          }`}
                        >
                          <span className={`font-pixel text-[10px] flex-1 truncate ${p.hasHadTurn ? "text-red-700" : "text-red-500"}`}>
                            {p.displayName}
                            {p.isHost && <span className="text-red-900/60"> [HOST]</span>}
                            {p.hasHadTurn && " ✓"}
                          </span>
                        </div>
                      ))}
                      {isTurnBased && (
                        <p className="text-red-900/40 font-pixel text-[8px] text-center uppercase pt-2">
                          {turnStatus.players.every(p => p.hasHadTurn)
                            ? "All players have selected"
                            : "Waiting for host to begin selections..."}
                        </p>
                      )}
                    </>
                  )}
                </CardContent>
              </Card>
            </div>
          )}

          {/* Odds Board */}
          <div className="flex-1 min-w-[280px] max-w-md">
            <Card className="bg-black border-4 border-red-900 rounded-none shadow-[0_0_20px_rgba(255,0,0,0.1)]">
              <CardHeader className="border-b-2 border-red-900 p-4">
                <CardTitle className="text-xl text-red-600 font-pixel text-center uppercase tracking-tighter">
                  THE ODDS
                </CardTitle>
              </CardHeader>
              <CardContent className="p-4 space-y-6">
                <div className="space-y-2">
                  <label className="text-[10px] text-red-900 font-pixel uppercase">
                    Multiplier
                  </label>
                  <div className="flex gap-2">
                    <Input
                      readOnly
                      value={tempMultiplier}
                      onChange={(e) =>
                        setTempMultiplier(parseInt(e.target.value))
                      }
                      className="bg-black border-2 border-red-900 text-red-500 font-mono text-center rounded-none h-9 focus-visible:ring-0 focus-visible:border-red-600"
                    />
                    {/* May make this button only visible to the host and build in rules for changing wager amount when players have already joined */}
                    {/* <Button
                      size="sm"
                      onClick={handleSetMultiplier}
                      className="bg-green-700 text-white font-pixel text-[10px] rounded-none hover:bg-green-600 h-9"
                    >
                      SET
                    </Button> */}
                  </div>
                </div>

                <div className="space-y-4">
                  <div className="grid grid-cols-3 text-[8px] text-red-900 font-pixel uppercase border-b border-red-900/30 pb-2">
                    <span>User</span>
                    <span className="text-center">Squares</span>
                    <span className="text-right">Wager</span>
                  </div>

                  <div className="max-h-[400px] overflow-y-auto space-y-3 custom-scrollbar">
                    {Object.entries(playerStats).length === 0 ? (
                      <div className="text-center py-4 text-red-900/40 font-pixel text-[8px] uppercase">
                        No Selections
                      </div>
                    ) : (
                      Object.entries(playerStats).map(([name, count]) => (
                        <motion.div
                          key={name}
                          initial={{ opacity: 0, x: 10 }}
                          animate={{ opacity: 1, x: 0 }}
                          className="grid grid-cols-3 text-[10px] font-pixel text-red-500 items-center"
                        >
                          <span className="truncate pr-1">{name}</span>
                          <span className="text-center">{count}</span>
                          <span className="text-right flex items-center justify-end gap-1">
                            <Coins size={10} className="text-yellow-600" />
                            {count * multiplier}
                          </span>
                        </motion.div>
                      ))
                    )}
                  </div>
                </div>

                <div className="pt-4 border-t-2 border-red-900 flex justify-between items-center">
                  <span className="text-[8px] text-red-900 font-pixel uppercase">
                    Total Pool
                  </span>
                  <span className="text-sm text-red-600 font-pixel flex items-center gap-1">
                    <Coins size={14} className="text-yellow-600" />
                    {Object.values(playerStats).reduce((a, b) => a + b, 0) *
                      multiplier}
                  </span>
                </div>
              </CardContent>
            </Card>
          </div>

        </div>
      </div>
    </div>
  );
}
