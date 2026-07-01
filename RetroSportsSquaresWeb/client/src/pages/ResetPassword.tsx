import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Link } from "wouter";
import { Loader2 } from "lucide-react";
import { Input } from "@/components/ui/input";
import { RetroButton } from "@/components/RetroButton";
import { API_BASE_URL, endpoints } from "@shared/routes";

export default function ResetPassword() {
  const queryClient = useQueryClient();
  const params = new URLSearchParams(window.location.search);
  const token = params.get("token") ?? "";
  const email = params.get("email") ?? "";

  const [form, setForm] = useState({ newPassword: "", confirmPassword: "" });
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!token || !email) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[50vh] gap-6 text-center max-w-sm mx-auto">
        <p className="font-['Press_Start_2P'] text-red-500 text-sm">INVALID LINK</p>
        <p className="font-['VT323'] text-gray-400 text-xl">
          THIS RESET LINK IS INVALID OR EXPIRED. PLEASE REQUEST A NEW ONE FROM SETTINGS.
        </p>
        <Link href="/settings">
          <RetroButton variant="outline" size="md">BACK TO SETTINGS</RetroButton>
        </Link>
      </div>
    );
  }

  const handleSubmit = async () => {
    setError(null);
    if (!form.newPassword || !form.confirmPassword) {
      setError("ALL FIELDS ARE REQUIRED");
      return;
    }
    if (form.newPassword !== form.confirmPassword) {
      setError("PASSWORDS DO NOT MATCH");
      return;
    }
    setSubmitting(true);
    try {
      const res = await fetch(`${API_BASE_URL}${endpoints.auth.resetPassword}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, token, newPassword: form.newPassword }),
      });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        throw new Error(data.message ?? "RESET FAILED. THE LINK MAY HAVE EXPIRED.");
      }
      localStorage.removeItem("token");
      queryClient.setQueryData(["currentUser"], null);
      setSuccess(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "SOMETHING WENT WRONG");
    } finally {
      setSubmitting(false);
    }
  };

  if (success) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[50vh] gap-6 text-center max-w-sm mx-auto">
        <p className="font-['Press_Start_2P'] text-primary text-sm">PASSWORD UPDATED!</p>
        <p className="font-['VT323'] text-gray-400 text-xl leading-tight">
          YOUR PASSWORD HAS BEEN CHANGED SUCCESSFULLY. LOG IN WITH YOUR NEW PASSWORD.
        </p>
        <Link href="/login">
          <RetroButton variant="primary" size="md">LOG IN</RetroButton>
        </Link>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-[50vh]">
      <div className="border-4 border-primary box-shadow-retro bg-black w-full max-w-sm">
        <div className="border-b-4 border-primary px-6 py-4">
          <h1 className="font-['Press_Start_2P'] text-primary text-xs text-shadow-retro">
            RESET PASSWORD
          </h1>
        </div>

        <div className="flex flex-col gap-4 px-6 py-5">
          {[
            { label: "NEW PASSWORD", key: "newPassword" as const },
            { label: "CONFIRM NEW PASSWORD", key: "confirmPassword" as const },
          ].map(({ label, key }) => (
            <div key={key} className="flex flex-col gap-1">
              <span className="font-['Press_Start_2P'] text-gray-500 text-[9px] tracking-wider">{label}</span>
              <Input
                type="password"
                value={form[key]}
                onChange={e => setForm(prev => ({ ...prev, [key]: e.target.value }))}
                disabled={submitting}
                onKeyDown={e => e.key === "Enter" && handleSubmit()}
                className="border-2 border-primary/30 bg-black text-white font-['VT323'] text-xl rounded-none
                  focus-visible:border-primary focus-visible:ring-0 focus-visible:ring-offset-0
                  disabled:opacity-60"
              />
            </div>
          ))}

          {error && (
            <p className="font-['Press_Start_2P'] text-red-500 text-[8px] leading-4">{error}</p>
          )}

          <RetroButton
            variant="primary"
            size="md"
            onClick={handleSubmit}
            disabled={submitting}
            className="w-full mt-1"
          >
            {submitting ? (
              <span className="flex items-center justify-center gap-2">
                <Loader2 className="w-3 h-3 animate-spin" />
                SAVING...
              </span>
            ) : (
              "SET NEW PASSWORD"
            )}
          </RetroButton>
        </div>
      </div>
    </div>
  );
}
