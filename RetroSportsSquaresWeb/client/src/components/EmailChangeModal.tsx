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

export function EmailChangeModal({ open, onClose }: Props) {
  const [form, setForm] = useState({ newEmail: "", currentPassword: "" });
  const [sending, setSending] = useState(false);
  const [sent, setSent] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    setError(null);
    if (!form.newEmail || !form.currentPassword) {
      setError("ALL FIELDS ARE REQUIRED");
      return;
    }
    setSending(true);
    try {
      const token = localStorage.getItem("token");
      const res = await fetch(`${API_BASE_URL}${endpoints.user.requestEmailChange}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ newEmail: form.newEmail, currentPassword: form.currentPassword }),
      });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        throw new Error(data.message ?? "REQUEST FAILED");
      }
      setSent(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "SOMETHING WENT WRONG");
    } finally {
      setSending(false);
    }
  };

  const handleOpenChange = (open: boolean) => {
    if (!open && !sending) {
      setForm({ newEmail: "", currentPassword: "" });
      setSent(false);
      setError(null);
      onClose();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-black border-4 border-primary box-shadow-retro rounded-none max-w-sm p-0">
        <DialogHeader className="border-b-4 border-primary px-6 py-4">
          <DialogTitle className="font-['Press_Start_2P'] text-primary text-xs text-shadow-retro">
            CHANGE EMAIL
          </DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-4 px-6 py-5">
          {sent ? (
            <>
              <p className="font-['VT323'] text-green-400 text-xl leading-tight">
                CONFIRMATION LINK SENT! CHECK YOUR NEW EMAIL INBOX AND CLICK THE LINK TO CONFIRM THE CHANGE.
              </p>
              <RetroButton variant="outline" size="md" onClick={onClose} className="w-full mt-1">
                CLOSE
              </RetroButton>
            </>
          ) : (
            <>
              {[
                { label: "NEW EMAIL ADDRESS", key: "newEmail" as const, type: "email" },
                { label: "CURRENT PASSWORD", key: "currentPassword" as const, type: "password" },
              ].map(({ label, key, type }) => (
                <div key={key} className="flex flex-col gap-1">
                  <span className="font-['Press_Start_2P'] text-gray-500 text-[9px] tracking-wider">{label}</span>
                  <Input
                    type={type}
                    value={form[key]}
                    onChange={e => setForm(prev => ({ ...prev, [key]: e.target.value }))}
                    disabled={sending}
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
                disabled={sending}
                className="w-full mt-1"
              >
                {sending ? (
                  <span className="flex items-center justify-center gap-2">
                    <Loader2 className="w-3 h-3 animate-spin" />
                    SENDING...
                  </span>
                ) : (
                  "SEND CONFIRMATION"
                )}
              </RetroButton>
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
