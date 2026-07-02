import { useLocation } from "wouter";
import { useAuth } from "@/hooks/use-auth";
import { Loader2 } from "lucide-react";

export function AdminRoute({ children }: { children: React.ReactNode }) {
  const { user, isLoading } = useAuth();
  const [, setLocation] = useLocation();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center gap-3 py-20">
        <Loader2 className="h-6 w-6 text-primary animate-spin" />
        <span className="font-['Press_Start_2P'] text-primary text-xs animate-pulse">LOADING...</span>
      </div>
    );
  }

  if (!user?.isAdmin) {
    setLocation("/");
    return null;
  }

  return <>{children}</>;
}
