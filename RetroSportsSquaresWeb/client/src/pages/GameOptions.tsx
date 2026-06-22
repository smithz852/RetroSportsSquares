import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "@/hooks/use-auth";
import Landing from "./Landing";
import { Button } from "@/components/ui/button";
import { Link } from "wouter";
import { Trophy, Circle, Globe } from "lucide-react";
import { motion } from "framer-motion";
import { useLocation } from "wouter";

export default function GameOptions() {
  const [, setLocation] = useLocation();
  const { user } = useAuth();

  if (user) {
return (
    <div className="flex flex-col items-center justify-center min-h-[80vh] gap-12 p-4">
      <motion.h1 
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        className="text-4xl md:text-6xl text-red-600 font-pixel text-center leading-tight uppercase tracking-tighter"
      >
        Select Your Arena
      </motion.h1>
      
      <div className="flex flex-wrap justify-center gap-8 w-full max-w-5xl">
        <Link href="/leagues/football" asChild>
          <motion.div
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            className="cursor-pointer w-80"
          >
            <Card className="bg-black border-4 border-red-600 rounded-none overflow-hidden group hover:shadow-[0_0_20px_rgba(255,0,0,0.5)] transition-all w-full h-full">
              <CardContent className="p-12 flex flex-col items-center gap-6">
                <div className="text-red-600 group-hover:animate-pulse">
                  <Trophy size={80} />
                </div>
                <h2 className="text-3xl text-red-600 font-pixel uppercase tracking-widest text-center">Football</h2>
                <p className="text-red-500/70 font-mono text-sm text-center">CLASSIC GRIDIRON SQUARES</p>
              </CardContent>
            </Card>
          </motion.div>
        </Link>

        <Link href="/leagues/basketball" asChild>
          <motion.div
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            className="cursor-pointer w-80"
          >
            <Card className="bg-black border-4 border-red-600 rounded-none overflow-hidden group hover:shadow-[0_0_20px_rgba(255,0,0,0.5)] transition-all w-full h-full">
              <CardContent className="p-12 flex flex-col items-center gap-6">
                <div className="text-red-600 group-hover:animate-bounce">
                  <Circle size={80} />
                </div>
                <h2 className="text-2xl text-red-600 font-pixel uppercase tracking-widest text-center">Basketball</h2>
                <p className="text-red-500/70 font-mono text-sm text-center">HARDWOOD HOOPS CHALLENGE</p>
              </CardContent>
            </Card>
          </motion.div>
        </Link>

        <Link href="/leagues/soccer" asChild>
          <motion.div
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            className="cursor-pointer w-80"
          >
            <Card className="bg-black border-4 border-red-600 rounded-none overflow-hidden group hover:shadow-[0_0_20px_rgba(255,0,0,0.5)] transition-all w-full h-full">
              <CardContent className="p-12 flex flex-col items-center gap-6">
                <div className="text-red-600 group-hover:animate-spin">
                  <Globe size={80} />
                </div>
                <h2 className="text-3xl text-red-600 font-pixel uppercase tracking-widest text-center">Soccer</h2>
                <p className="text-red-500/70 font-mono text-sm text-center">GLOBAL FOOTBALL SQUARES</p>
              </CardContent>
            </Card>
          </motion.div>
        </Link>

        
      </div>
    </div>
  );
  } 
  else
  {
    setLocation("/login")
  }
  
}
