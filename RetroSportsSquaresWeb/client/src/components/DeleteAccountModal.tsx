import { useRef, useState } from "react";
import { useLocation } from "wouter";
import { useQueryClient } from "@tanstack/react-query";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { RetroButton } from "@/components/RetroButton";
import { Loader2 } from "lucide-react";
import { API_BASE_URL, endpoints } from "@shared/routes";
import { getCsrfToken } from "@/lib/api-client";

type Props = {
  open: boolean;
  onClose: () => void;
};

export function DeleteAccountModal({ open, onClose }: Props) {
  const [form, setForm] = useState({ currentPassword: "", confirmText: "" });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [, setLocation] = useLocation();
  const queryClient = useQueryClient();
  const currentPasswordRef = useRef<HTMLInputElement>(null);
  const confirmTextRef = useRef<HTMLInputElement>(null);
  const errorRef = useRef<HTMLParagraphElement>(null);

  const handleSubmit = async () => {
    setError(null);
    if (!form.currentPassword || form.confirmText !== "DELETE") {
      setError("ENTER YOUR PASSWORD AND TYPE DELETE TO CONFIRM");
      (form.currentPassword ? confirmTextRef : currentPasswordRef).current?.focus();
      return;
    }
    setSubmitting(true);
    try {
      const csrfToken = getCsrfToken();
      const res = await fetch(`${API_BASE_URL}${endpoints.user.deleteAccount}`, {
        method: "DELETE",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
          ...(csrfToken ? { "X-XSRF-TOKEN": csrfToken } : {}),
        },
        body: JSON.stringify({ currentPassword: form.currentPassword, confirmText: form.confirmText }),
      });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        throw new Error(data.message ?? "DELETE FAILED");
      }

      queryClient.setQueryData(["currentUser"], null);
      setLocation("/");
    } catch (err) {
      setError(err instanceof Error ? err.message : "SOMETHING WENT WRONG");
      errorRef.current?.focus();
      setSubmitting(false);
    }
  };

  const handleOpenChange = (open: boolean) => {
    if (!open && !submitting) {
      setForm({ currentPassword: "", confirmText: "" });
      setError(null);
      onClose();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-black border-4 border-red-600 box-shadow-retro rounded-none max-w-sm p-0">
        <DialogHeader className="border-b-4 border-red-600 px-6 py-4">
          <DialogTitle className="font-['Press_Start_2P'] text-red-500 text-xs text-shadow-retro">
            DELETE ACCOUNT
          </DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-4 px-6 py-5">
          <p className="font-['VT323'] text-gray-400 text-lg leading-tight">
            THIS PERMANENTLY DELETES YOUR ACCOUNT. ANY OPEN GAMES YOU HOST WILL BE
            CANCELLED AND REFUNDED. THIS CANNOT BE UNDONE.
          </p>

          <div className="flex flex-col gap-1">
            <span className="font-['Press_Start_2P'] text-gray-500 text-[10px] tracking-wider">CURRENT PASSWORD</span>
            <Input
              ref={currentPasswordRef}
              type="password"
              value={form.currentPassword}
              onChange={e => setForm(prev => ({ ...prev, currentPassword: e.target.value }))}
              disabled={submitting}
              className="border-2 border-primary/30 bg-black text-white font-['VT323'] text-xl rounded-none
                focus-visible:border-primary focus-visible:ring-0 focus-visible:ring-offset-0
                disabled:opacity-60"
            />
          </div>

          <div className="flex flex-col gap-1">
            <span className="font-['Press_Start_2P'] text-gray-500 text-[10px] tracking-wider">TYPE DELETE TO CONFIRM</span>
            <Input
              ref={confirmTextRef}
              type="text"
              value={form.confirmText}
              onChange={e => setForm(prev => ({ ...prev, confirmText: e.target.value }))}
              disabled={submitting}
              className="border-2 border-red-600/40 bg-black text-white font-['VT323'] text-xl rounded-none
                focus-visible:border-red-600 focus-visible:ring-0 focus-visible:ring-offset-0
                disabled:opacity-60 uppercase"
            />
          </div>

          {error && (
            <p
              ref={errorRef}
              role="alert"
              aria-live="polite"
              tabIndex={-1}
              className="font-['Press_Start_2P'] text-red-500 text-[10px] leading-4"
            >
              {error}
            </p>
          )}

          <RetroButton
            variant="outline"
            size="md"
            onClick={handleSubmit}
            disabled={submitting}
            className="w-full mt-1 border-red-600 text-red-500 hover:bg-red-600 hover:text-black"
          >
            {submitting ? (
              <span className="flex items-center justify-center gap-2">
                <Loader2 className="w-3 h-3 animate-spin" />
                DELETING...
              </span>
            ) : (
              "DELETE MY ACCOUNT"
            )}
          </RetroButton>
        </div>
      </DialogContent>
    </Dialog>
  );
}
