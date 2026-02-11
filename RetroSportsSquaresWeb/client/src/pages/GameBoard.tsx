import { useState, useEffect } from "react";
import { useParams, Link } from "wouter";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { motion } from "framer-motion";
import { useToast } from "@/hooks/use-toast";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Coins } from "lucide-react";
import { Scoreboard } from "@/components/Scoreboard";
import { useAuth } from "@/hooks/use-auth";
import { getSquareGameById } from "@/hooks/use-games";
import { number } from "zod";

export default function GameBoard() {
  const { user, isLoading: authLoading } = useAuth();
  const params = useParams();
  const id = params.id as string;
  const { toast } = useToast();
  
  const { data: game, isLoading: gameLoading, error } = getSquareGameById(id);

  const [topNumbers, setTopNumbers] = useState<(number | null)[]>(Array(10).fill(null));
  const [leftNumbers, setLeftNumbers] = useState<(number | null)[]>(Array(10).fill(null));
  const [selections, setSelections] = useState<Record<string, string>>({});
  const [gameStarted, setGameStarted] = useState(false);
  const [homeTeam, setHomeTeam] = useState("");
  const [awayTeam, setAwayTeam] = useState("");

  
  const [activePlayer, setActivePlayer] = useState(() => {
    return localStorage.getItem("sports_squares_player") || "";
  });
  const [tempPlayerName, setTempPlayerName] = useState(activePlayer);

  // Odds Board State
  const [multiplier, setMultiplier] = useState(0);
  const [tempMultiplier, setTempMultiplier] = useState(0);
  
  // Update state when game data loads
  useEffect(() => {
    if (game) {
      setHomeTeam(game.homeTeam || "");
      setAwayTeam(game.awayTeam || "");
      setMultiplier(game.pricePerSquare || 0);
      setTempMultiplier(game.pricePerSquare || 0);
    }
  }, [game]);
  
  // Redirect if not authenticated
  if (authLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-black">
        <h2 className="text-primary font-pixel animate-pulse">AUTHENTICATING...</h2>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-black gap-6">
        <h2 className="text-red-500 font-pixel text-center">ACCESS DENIED - LOGIN REQUIRED</h2>
        <Link href="/login">
          <Button className="bg-red-600 text-black font-pixel py-4 px-8 rounded-none hover:bg-red-500 active:translate-y-1 transition-all uppercase">
            LOGIN
          </Button>
        </Link>
      </div>
    );
  }
  
  // Validate game ID (GUID format)
  const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
  if (!id || !guidRegex.test(id)) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-black">
        <h2 className="text-red-500 font-pixel">INVALID GAME ID</h2>
      </div>
    );
  }

  const handleSetPlayer = () => {
    setActivePlayer(tempPlayerName);
    localStorage.setItem("sports_squares_player", tempPlayerName);
    toast({ 
      title: "PLAYER SET", 
      description: `Active player: ${tempPlayerName || 'NONE'}` 
    });
  };

  const handleSetMultiplier = () => {
    if (tempMultiplier >= 0) {
      setMultiplier(tempMultiplier);
      toast({ title: "MULTIPLIER SET", description: `Wager per square: ${tempMultiplier} coins` });
    }
  };

  const generateNumbers = () => {
    const nums = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
    setTopNumbers([...nums].sort(() => Math.random() - 0.5));
    setLeftNumbers([...nums].sort(() => Math.random() - 0.5));
    toast({ title: "NUMBERS GENERATED", description: "Random numbers assigned to red squares." });
  };

  const clearNumbers = () => {
    setTopNumbers(Array(10).fill(null));
    setLeftNumbers(Array(10).fill(null));
  };

  const clearSelections = () => {
    setSelections({});
  };

  const handleSquareClick = (row: number, col: number) => {
    if (gameStarted) return;
    const key = `${row}-${col}`;
    if (selections[key]) {
      const newSelections = { ...selections };
      delete newSelections[key];
      setSelections(newSelections);
    } else {
      if (!activePlayer) {
        toast({ 
          title: "PLAYER REQUIRED", 
          description: "Please enter and submit a username above the board first!",
          variant: "destructive"
        });
        return;
      }
      setSelections({ ...selections, [key]: activePlayer });
    }
  };

  if (gameLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-black">
        <h2 className="text-primary font-pixel animate-pulse">LOADING GAME...</h2>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-black">
        <h2 className="text-red-500 font-pixel">ERROR: {(error as Error).message}</h2>
      </div>
    );
  }

  // Calculate odds data
  const playerStats = Object.values(selections).reduce((acc, name) => {
    acc[name] = (acc[name] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);


  return (
    <div className="flex flex-col items-center p-4 max-w-[1400px] mx-auto w-full">
       <Scoreboard isVisible={gameStarted} gameName={(game as any)?.name} squareGameId={id} gameStartTime={game?.startTime} />
      
      <div className="flex flex-col lg:flex-row items-start justify-center gap-8 w-full">
        <div className="flex flex-col items-center gap-8 flex-1 w-full">
          <div className="flex flex-col items-center gap-4 w-full max-w-xl">
            <div className="flex gap-2 w-full">
              <Input 
                value={tempPlayerName}
                onChange={(e) => setTempPlayerName(e.target.value)}
                placeholder="Enter Username"
                className="bg-black border-2 border-red-600 text-red-500 font-pixel text-xs rounded-none h-12 focus-visible:ring-0 focus-visible:border-red-400 placeholder:text-red-900/50"
              />
              <Button 
                onClick={handleSetPlayer}
                className="bg-red-600 text-black font-pixel h-12 rounded-none hover:bg-red-500 active:translate-y-1 transition-all uppercase px-6"
              >
                submit
              </Button>
            </div>

            {!gameStarted ? (
              <>
                <Button 
                  onClick={() => {
                    setGameStarted(true)
                  }}
                  className="w-full bg-red-600 text-black font-pixel text-xl py-8 rounded-none border-b-8 border-red-900 active:border-b-0 active:translate-y-2 transition-all hover:bg-red-500 animate-pulse"
                >
                  INSERT COIN / START GAME
                </Button>
                <div className="flex gap-4">
                  <Button onClick={generateNumbers} variant="outline" className="border-2 border-red-600 text-red-600 font-pixel text-xs rounded-none hover:bg-red-600 hover:text-black">
                    GENERATE NUMS
                  </Button>
                  <Button onClick={clearNumbers} variant="ghost" className="text-red-900 font-pixel text-[10px] rounded-none hover:text-red-500">
                    CLEAR NUMS
                  </Button>
                  <Button onClick={clearSelections} variant="ghost" className="text-red-900 font-pixel text-[10px] rounded-none hover:text-red-500">
                    CLEAR SELECTIONS
                  </Button>
                </div>
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
                <span className="text-red-600 font-pixel text-sm">{homeTeam}</span>
              </div>
              
              {/* Game Grid */}
              <div className="inline-grid grid-cols-11 border-4 border-red-900 bg-black p-1 shadow-[0_0_30px_rgba(255,0,0,0.2)]">
                <div 
              onClick={() => { if(confirm("RESET GAME?")) { setGameStarted(false); clearNumbers(); clearSelections(); }}}
              className="w-10 h-10 md:w-14 md:h-14 bg-red-600 border-2 border-red-900 flex items-center justify-center cursor-pointer animate-[pulse_2s_infinite] hover:bg-red-500 transition-colors"
            >
              <span className="text-black font-pixel text-[8px] md:text-[10px]">RESET</span>
            </div>

            {topNumbers.map((num, i) => (
              <div key={`top-${i}`} className="w-10 h-10 md:w-14 md:h-14 bg-red-600 border-2 border-red-900 flex items-center justify-center font-pixel text-black text-xl">
                {num !== null ? num : "?"}
              </div>
            ))}

            {Array.from({ length: 10 }).map((_, rowIndex) => (
              <div key={`row-${rowIndex}`} className="contents">
                <div className="w-10 h-10 md:w-14 md:h-14 bg-red-600 border-2 border-red-900 flex items-center justify-center font-pixel text-black text-xl">
                  {leftNumbers[rowIndex] !== null ? leftNumbers[rowIndex] : "?"}
                </div>

                {Array.from({ length: 10 }).map((_, colIndex) => {
                  const squareId = `${rowIndex}-${colIndex}`;
                  const name = selections[squareId];
                  return (
                    <div
                      key={squareId}
                      data-square-id={squareId}
                      onClick={() => handleSquareClick(rowIndex, colIndex)}
                      className={`w-10 h-10 md:w-14 md:h-14 border-2 border-red-900/30 flex flex-col items-center justify-center cursor-pointer transition-all ${
                        name ? 'bg-red-600/20' : 'hover:bg-red-900/10'
                      }`}
                    >
                      <span className={`font-pixel text-[6px] md:text-[8px] text-center px-1 leading-tight ${name ? 'text-red-500' : 'text-red-900/40'}`}>
                        {name || "OPEN"}
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

        {/* Odds Board */}
        <div className="w-full lg:w-80 shrink-0">
          <Card className="bg-black border-4 border-red-900 rounded-none shadow-[0_0_20px_rgba(255,0,0,0.1)]">
            <CardHeader className="border-b-2 border-red-900 p-4">
              <CardTitle className="text-xl text-red-600 font-pixel text-center uppercase tracking-tighter">
                THE ODDS
              </CardTitle>
            </CardHeader>
            <CardContent className="p-4 space-y-6">
              <div className="space-y-2">
                <label className="text-[10px] text-red-900 font-pixel uppercase">Multiplier</label>
                <div className="flex gap-2">
                  <Input 
                    value={tempMultiplier}
                    onChange={(e) => setTempMultiplier(parseInt(e.target.value))}
                    className="bg-black border-2 border-red-900 text-red-500 font-mono text-center rounded-none h-9 focus-visible:ring-0 focus-visible:border-red-600"
                  />
                  <Button 
                    size="sm"
                    onClick={handleSetMultiplier}
                    className="bg-green-700 text-white font-pixel text-[10px] rounded-none hover:bg-green-600 h-9"
                  >
                    SET
                  </Button>
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
                <span className="text-[8px] text-red-900 font-pixel uppercase">Total Pool</span>
                <span className="text-sm text-red-600 font-pixel flex items-center gap-1">
                  <Coins size={14} className="text-yellow-600" />
                  {Object.values(playerStats).reduce((a, b) => a + b, 0) * multiplier}
                </span>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
