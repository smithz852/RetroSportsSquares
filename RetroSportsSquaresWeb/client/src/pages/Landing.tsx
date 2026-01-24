import { RetroButton } from "@/components/RetroButton";
import { motion } from "framer-motion";
import { Trophy, Users, ShieldCheck } from "lucide-react";

export default function Landing() {
  const handleLogin = () => {
    window.location.href = "/login";
  };

  return (
    <div className="flex flex-col items-center">
      {/* Hero Section */}
      <section className="w-full py-20 md:py-32 flex flex-col items-center text-center relative overflow-hidden">
        {/* Background Grid Animation Effect */}
        <div className="absolute inset-0 z-0 opacity-20 pointer-events-none">
          <div className="w-full h-full" 
               style={{ 
                 backgroundImage: 'linear-gradient(var(--primary) 1px, transparent 1px), linear-gradient(90deg, var(--primary) 1px, transparent 1px)', 
                 backgroundSize: '40px 40px',
                 perspective: '500px',
                 transform: 'rotateX(60deg) scale(2)',
                 transformOrigin: 'top center'
               }} 
          />
        </div>

        <motion.div 
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ duration: 0.5, type: "spring" }}
          className="z-10 relative mb-8"
        >
          <div className="w-32 h-32 md:w-48 md:h-48 bg-primary mx-auto mb-8 rotate-45 border-4 border-white shadow-[0_0_50px_rgba(255,0,0,0.5)] flex items-center justify-center">
             <Trophy className="w-16 h-16 md:w-24 md:h-24 text-black -rotate-45" />
          </div>
        </motion.div>

        <motion.h1 
          initial={{ y: 20, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          transition={{ delay: 0.2 }}
          className="text-4xl md:text-6xl lg:text-7xl font-['Press_Start_2P'] text-primary mb-6 text-shadow-retro max-w-4xl px-4 z-10"
        >
          SPORTS<br/>SQUARES
        </motion.h1>

        <motion.p 
          initial={{ y: 20, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          transition={{ delay: 0.4 }}
          className="text-xl md:text-2xl font-['VT323'] text-gray-300 mb-12 max-w-2xl px-4 z-10"
        >
          THE ULTIMATE 8-BIT GRIDIRON EXPERIENCE. JOIN A GAME, PICK YOUR SQUARES, AND DOMINATE THE BOARD.
        </motion.p>

        <motion.div
          initial={{ y: 20, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          transition={{ delay: 0.6 }}
          className="z-10"
        >
          <RetroButton size="lg" onClick={handleLogin} className="text-xl animate-pulse">
            INSERT COIN TO START
          </RetroButton>
        </motion.div>
      </section>

      {/* Features Grid */}
      <section className="w-full max-w-7xl px-4 py-16 grid grid-cols-1 md:grid-cols-3 gap-8">
        <motion.div 
          whileHover={{ y: -5 }}
          className="border-2 border-zinc-800 bg-zinc-900/50 p-8 text-center"
        >
          <div className="w-16 h-16 bg-primary/20 rounded-none mx-auto mb-6 flex items-center justify-center border-2 border-primary">
            <Trophy className="text-primary w-8 h-8" />
          </div>
          <h3 className="text-primary font-['Press_Start_2P'] text-sm mb-4">INSTANT ACTION</h3>
          <p className="font-['VT323'] text-xl text-gray-400">Create games instantly and invite friends with a single link.</p>
        </motion.div>

        <motion.div 
          whileHover={{ y: -5 }}
          className="border-2 border-zinc-800 bg-zinc-900/50 p-8 text-center"
        >
          <div className="w-16 h-16 bg-primary/20 rounded-none mx-auto mb-6 flex items-center justify-center border-2 border-primary">
            <Users className="text-primary w-8 h-8" />
          </div>
          <h3 className="text-primary font-['Press_Start_2P'] text-sm mb-4">MULTIPLAYER</h3>
          <p className="font-['VT323'] text-xl text-gray-400">Compete against up to 100 players per grid. Real-time updates.</p>
        </motion.div>

        <motion.div 
          whileHover={{ y: -5 }}
          className="border-2 border-zinc-800 bg-zinc-900/50 p-8 text-center"
        >
          <div className="w-16 h-16 bg-primary/20 rounded-none mx-auto mb-6 flex items-center justify-center border-2 border-primary">
            <ShieldCheck className="text-primary w-8 h-8" />
          </div>
          <h3 className="text-primary font-['Press_Start_2P'] text-sm mb-4">SECURE LOGIN</h3>
          <p className="font-['VT323'] text-xl text-gray-400">Powered by Replit Auth. No passwords to remember. Just play.</p>
        </motion.div>
      </section>

      {/* Footer */}
      <footer className="w-full py-8 border-t-2 border-zinc-800 mt-12 text-center">
        <p className="font-['VT323'] text-gray-600 text-lg">
          © 2024 SPORTS SQUARES. ALL RIGHTS RESERVED.
        </p>
      </footer>
    </div>
  );
}
