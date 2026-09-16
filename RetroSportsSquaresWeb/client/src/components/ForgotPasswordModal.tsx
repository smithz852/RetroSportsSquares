import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { RetroButton } from "@/components/RetroButton";
import { Loader2 } from "lucide-react";
import { API_BASE_URL, endpoints } from "@shared/routes";

type Props = {
  open: boolean;
  onClose: () => void;
};

export function ForgotPasswordModal({ open, onClose }: Props) {
  const [email, setEmail] = useState("");
  const [sending, setSending] = useState(false);
  const [sent, setSent] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSend = async () => {
    if (!email.trim()) {
      setError("EMAIL IS REQUIRED");
      return;
    }
    setError(null);
    setSending(true);
    try {
      const res = await fetch(`${API_BASE_URL}${endpoints.auth.forgotPassword}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email: email.trim() }),
      });
      if (!res.ok) throw new Error();
      setSent(true);
    } catch {
      setError("SOMETHING WENT WRONG. PLEASE TRY AGAIN.");
    } finally {
      setSending(false);
    }
  };

  const handleOpenChange = (open: boolean) => {
    if (!open && !sending) {
      setEmail("");
      setSent(false);
      setError(null);
      onClose();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-black border-4 border-red-600 box-shadow-retro rounded-none max-w-sm p-0">
        <DialogHeader className="border-b-4 border-red-600 px-6 py-4">
          <DialogTitle className="font-['Press_Start_2P'] text-red-600 text-[12px] text-shadow-retro">
            RESET PASSWORD
          </DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-4 px-6 py-5">
          {sent ? (
            <>
              <p className="font-['VT323'] text-green-400 text-xl leading-tight">
                RESET LINK SENT! CHECK YOUR INBOX AND FOLLOW THE LINK TO SET A NEW PASSWORD.
              </p>
              <RetroButton variant="outline" size="md" onClick={onClose} className="w-full mt-1">
                CLOSE
              </RetroButton>
            </>
          ) : (
            <>
              <p className="font-['VT323'] text-gray-400 text-xl leading-tight">
                ENTER YOUR EMAIL AND WE'LL SEND A PASSWORD RESET LINK.
              </p>
              <Input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="PLAYER@ARCADE.COM"
                disabled={sending}
                onKeyDown={(e) => e.key === "Enter" && handleSend()}
                className="bg-black border-2 border-red-900 text-red-500 font-mono rounded-none focus:border-red-600 focus:ring-0 placeholder:text-red-900"
              />
              {error && (
                <p className="font-['Press_Start_2P'] text-red-500 text-[10px] leading-4">{error}</p>
              )}
              <RetroButton
                variant="primary"
                size="md"
                onClick={handleSend}
                disabled={sending}
                className="w-full mt-1"
              >
                {sending ? (
                  <span className="flex items-center justify-center gap-2">
                    <Loader2 className="w-3 h-3 animate-spin" />
                    SENDING...
                  </span>
                ) : (
                  "SEND RESET LINK"
                )}
              </RetroButton>
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
