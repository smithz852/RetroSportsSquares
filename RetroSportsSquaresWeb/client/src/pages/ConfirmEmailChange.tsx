import { useEffect, useState } from "react";
import { Link } from "wouter";
import { Loader2 } from "lucide-react";
import { RetroButton } from "@/components/RetroButton";
import { API_BASE_URL, endpoints } from "@shared/routes";

export default function ConfirmEmailChange() {
  const [status, setStatus] = useState<"loading" | "success" | "unauthorized" | "error">("loading");
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get("token");
    const newEmail = params.get("email");

    if (!token || !newEmail) {
      setErrorMsg("INVALID CONFIRMATION LINK.");
      setStatus("error");
      return;
    }

    const jwt = localStorage.getItem("token");
    if (!jwt) {
      setStatus("unauthorized");
      return;
    }

    fetch(`${API_BASE_URL}${endpoints.user.confirmEmailChange}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${jwt}`,
      },
      body: JSON.stringify({ newEmail, token }),
    })
      .then(async (res) => {
        if (res.status === 401) {
          setStatus("unauthorized");
        } else if (!res.ok) {
          const data = await res.json().catch(() => ({}));
          setErrorMsg(data.message ?? "CONFIRMATION FAILED. THE LINK MAY HAVE EXPIRED.");
          setStatus("error");
        } else {
          localStorage.removeItem("token");
          setStatus("success");
        }
      })
      .catch(() => {
        setErrorMsg("COULD NOT CONNECT TO SERVER.");
        setStatus("error");
      });
  }, []);

  return (
    <div className="flex flex-col items-center justify-center min-h-[50vh] gap-6">
      {status === "loading" && (
        <div className="flex flex-col items-center gap-4">
          <Loader2 className="w-8 h-8 text-primary animate-spin" />
          <p className="font-['Press_Start_2P'] text-primary text-sm">CONFIRMING...</p>
        </div>
      )}

      {status === "success" && (
        <div className="flex flex-col items-center gap-6 text-center max-w-sm">
          <p className="font-['Press_Start_2P'] text-primary text-sm">EMAIL UPDATED!</p>
          <p className="font-['VT323'] text-gray-400 text-xl leading-tight">
            YOUR EMAIL ADDRESS HAS BEEN CHANGED SUCCESSFULLY. PLEASE LOG IN AGAIN WITH YOUR NEW EMAIL.
          </p>
          <Link href="/login">
            <RetroButton variant="primary" size="md">LOG IN</RetroButton>
          </Link>
        </div>
      )}

      {status === "unauthorized" && (
        <div className="flex flex-col items-center gap-6 text-center max-w-sm">
          <p className="font-['Press_Start_2P'] text-yellow-400 text-sm">LOGIN REQUIRED</p>
          <p className="font-['VT323'] text-gray-400 text-xl leading-tight">
            PLEASE LOG IN FIRST, THEN CLICK THE CONFIRMATION LINK AGAIN.
          </p>
          <Link href="/login">
            <RetroButton variant="primary" size="md">LOG IN</RetroButton>
          </Link>
        </div>
      )}

      {status === "error" && (
        <div className="flex flex-col items-center gap-6 text-center max-w-sm">
          <p className="font-['Press_Start_2P'] text-red-500 text-sm">CONFIRMATION FAILED</p>
          <p className="font-['VT323'] text-gray-400 text-xl leading-tight">{errorMsg}</p>
          <Link href="/settings">
            <RetroButton variant="outline" size="md">BACK TO SETTINGS</RetroButton>
          </Link>
        </div>
      )}
    </div>
  );
}
