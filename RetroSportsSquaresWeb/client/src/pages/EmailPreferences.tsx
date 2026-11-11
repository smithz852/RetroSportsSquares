import { useEffect, useRef, useState } from "react";
import { Link } from "wouter";
import { Loader2 } from "lucide-react";
import { RetroButton } from "@/components/RetroButton";
import { useAuth } from "@/hooks/use-auth";
import { API_BASE_URL, endpoints } from "@shared/routes";

export default function EmailPreferences() {
  const { user } = useAuth();
  const params = new URLSearchParams(window.location.search);
  const userId = params.get("u") ?? "";
  const token = params.get("t") ?? "";

  const [status, setStatus] = useState<"pending" | "done" | "error">(userId && token ? "pending" : "error");
  const ranRef = useRef(false);

  useEffect(() => {
    if (!userId || !token || ranRef.current) return;
    ranRef.current = true;

    (async () => {
      try {
        const res = await fetch(`${API_BASE_URL}${endpoints.user.unsubscribe}`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ userId, token }),
        });
        setStatus(res.ok ? "done" : "error");
      } catch {
        setStatus("error");
      }
    })();
  }, [userId, token]);

  return (
    <div className="flex flex-col items-center justify-center min-h-[50vh] gap-6 text-center max-w-sm mx-auto">
      {status === "pending" && (
        <>
          <Loader2 className="w-6 h-6 animate-spin text-primary" />
          <p className="font-['VT323'] text-gray-400 text-xl">UPDATING YOUR EMAIL PREFERENCES...</p>
        </>
      )}

      {status === "done" && (
        <>
          <p className="font-['Press_Start_2P'] text-primary text-sm">UNSUBSCRIBED</p>
          <p className="font-['VT323'] text-gray-400 text-xl leading-tight">
            YOU WON'T RECEIVE GAME RECAP EMAILS ANYMORE.
          </p>
          <Link href={user ? "/settings" : "/login"}>
            <RetroButton variant="outline" size="md">
              {user ? "MANAGE EMAIL PREFERENCES" : "BACK TO LOGIN"}
            </RetroButton>
          </Link>
        </>
      )}

      {status === "error" && (
        <>
          <p className="font-['Press_Start_2P'] text-red-500 text-sm">INVALID LINK</p>
          <p className="font-['VT323'] text-gray-400 text-xl leading-tight">
            THIS UNSUBSCRIBE LINK IS INVALID. YOU CAN MANAGE EMAIL PREFERENCES FROM SETTINGS INSTEAD.
          </p>
          <Link href={user ? "/settings" : "/login"}>
            <RetroButton variant="outline" size="md">
              {user ? "GO TO SETTINGS" : "LOG IN"}
            </RetroButton>
          </Link>
        </>
      )}
    </div>
  );
}
