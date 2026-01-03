import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Link } from "wouter";
import { Trophy, Circle } from "lucide-react";
import { motion } from "framer-motion";

export default function GameOptions() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[80vh] gap-12 p-4">
      <motion.h1 
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        className="text-4xl md:text-6xl text-red-600 font-pixel text-center leading-tight uppercase tracking-tighter"
      >
        Select Your Arena
      </motion.h1>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 w-full max-w-4xl">
        <Link href="/arena/football">
          <motion.div
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            className="cursor-pointer"
          >
            <Card className="bg-black border-4 border-red-600 rounded-none overflow-hidden group hover:shadow-[0_0_20px_rgba(255,0,0,0.5)] transition-all">
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

        <Link href="/arena/basketball">
          <motion.div
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            className="cursor-pointer"
          >
            <Card className="bg-black border-4 border-red-600 rounded-none overflow-hidden group hover:shadow-[0_0_20px_rgba(255,0,0,0.5)] transition-all">
              <CardContent className="p-12 flex flex-col items-center gap-6">
                <div className="text-red-600 group-hover:animate-bounce">
                  <Circle size={80} />
                </div>
                <h2 className="text-3xl text-red-600 font-pixel uppercase tracking-widest text-center">Basketball</h2>
                <p className="text-red-500/70 font-mono text-sm text-center">HARDWOOD HOOPS CHALLENGE</p>
              </CardContent>
            </Card>
          </motion.div>
        </Link>
      </div>
    </div>
  );
}
