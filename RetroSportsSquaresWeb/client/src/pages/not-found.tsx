import { Link } from "wouter";
import { AlertTriangle } from "lucide-react";
import { RetroButton } from "@/components/RetroButton";

export default function NotFound() {
  return (
    <div className="min-h-[80vh] w-full flex flex-col items-center justify-center bg-black text-center px-4">
      <div className="mb-8 relative">
        <AlertTriangle className="h-32 w-32 text-red-600 animate-pulse" />
        <div className="absolute inset-0 border-4 border-red-600 animate-ping opacity-20"></div>
      </div>
      
      <h1 className="text-4xl md:text-6xl font-['Press_Start_2P'] text-red-600 mb-6 text-shadow-retro">
        404 ERROR
      </h1>
      
      <p className="text-xl md:text-2xl font-['VT323'] text-red-400 mb-12 max-w-lg">
        THE LEVEL YOU ARE LOOKING FOR HAS NOT BEEN UNLOCKED OR DOES NOT EXIST.
      </p>

      <Link href="/">
        <RetroButton size="lg">RETURN TO BASE</RetroButton>
      </Link>
    </div>
  );
}
