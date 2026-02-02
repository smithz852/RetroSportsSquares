import { Link } from "wouter";
import { useAuth } from "@/hooks/use-auth";
import { RetroButton } from "./RetroButton";
import { Gamepad2, LogOut, User } from "lucide-react";

export function Navbar() {
  const { user, logout, isLoggingOut } = useAuth();

  const handleLogin = () => {
    window.location.href = "../pages/Login";
  };

  return (
    <nav className="border-b-4 border-primary bg-black sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-20 items-center">
          <Link href="/" className="flex items-center gap-3 group cursor-pointer">
            <div className="p-2 border-2 border-primary group-hover:bg-primary group-hover:text-black transition-colors">
              <Gamepad2 className="h-8 w-8" />
            </div>
            <span className="font-['Press_Start_2P'] text-lg md:text-xl text-primary text-shadow-retro hidden sm:block">
              SPORTS SQUARES
            </span>
          </Link>

          <div className="flex items-center gap-4">
            {user ? (
              <>
                <div className="hidden md:flex items-center gap-2 text-primary font-['VT323'] text-xl border-2 border-primary/30 px-3 py-1 bg-primary/5">
                  <User className="h-4 w-4" />
                  <span>PLAYER: {user.displayName || 'UNKNOWN'}</span>
                </div>
                <Link href="/options">
                  <RetroButton variant="outline" size="sm">
                    ARENA
                  </RetroButton>
                </Link>
                <RetroButton 
                  variant="outline" 
                  size="sm" 
                  onClick={() => logout()}
                  disabled={isLoggingOut}
                >
                  {isLoggingOut ? "EXITING..." : "LOGOUT"}
                </RetroButton>
              </>
            ) : (
             <div className="flex items-center gap-2">
                <RetroButton variant="primary" size="sm" onClick={handleLogin}>
                  LOGIN
                </RetroButton>
                <Link href="/signup">
                  <RetroButton variant="outline" size="sm">
                    SIGN UP
                  </RetroButton>
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}
