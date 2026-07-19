import { Link } from "wouter";
import { useAuth } from "@/hooks/use-auth";
import { RetroButton } from "./RetroButton";
import { Coins, Gamepad2, LogOut, User } from "lucide-react";

// Whole coins render bare ("45"), fractional balances keep two decimals ("45.50")
function formatCoins(balance: number): string {
  return Number.isInteger(balance) ? balance.toString() : balance.toFixed(2);
}

export function Navbar() {
  const { user, logout, isLoggingOut } = useAuth();

  const handleLogout = () => {
    // Navigate only after the token is cleared, or the reload races the removal
    logout(undefined, {
      onSettled: () => {
        window.location.href = "/";
      },
    });
  };

  const handleLogin = () => {
    window.location.href = "/Login";
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
                <div
                  className="flex items-center gap-2 text-yellow-400 font-['VT323'] text-xl border-2 border-yellow-400/30 px-3 py-1 bg-yellow-400/5"
                  title="Coin balance — 15 free coins per active day"
                >
                  <Coins className="h-4 w-4" />
                  <span>{formatCoins(user.coinBalance ?? 0)}</span>
                </div>
                <Link href="/options">
                  <RetroButton variant="outline" size="sm">
                    ARENA
                  </RetroButton>
                </Link>
                <Link href="/player-dashboard">
                  <RetroButton variant="outline" size="sm">
                    PROFILE
                  </RetroButton>
                </Link>
                {user.isAdmin && (
                  <Link href="/admin">
                    <RetroButton variant="outline" size="sm">
                      ADMIN
                    </RetroButton>
                  </Link>
                )}
                <RetroButton 
                  variant="outline" 
                  size="sm" 
                  onClick={handleLogout}
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
