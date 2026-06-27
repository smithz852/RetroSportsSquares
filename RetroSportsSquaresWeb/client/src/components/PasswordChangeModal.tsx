import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { RetroButton } from "@/components/RetroButton";
import { Loader2 } from "lucide-react";

type Props = {
  open: boolean;
  onClose: () => void;
};

export function PasswordChangeModal({ open, onClose }: Props) {
  const [form, setForm] = useState({ current: "", newPw: "", repeatPw: "" });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    setError(null);
    if (form.newPw !== form.repeatPw) {
      setError("NEW PASSWORDS DO NOT MATCH");
      return;
    }
    if (!form.current || !form.newPw) {
      setError("ALL FIELDS ARE REQUIRED");
      return;
    }
    setSaving(true);
    try {
      // TODO: wire to API
      await new Promise(res => setTimeout(res, 800));
      setForm({ current: "", newPw: "", repeatPw: "" });
      onClose();
    } finally {
      setSaving(false);
    }
  };

  const handleOpenChange = (open: boolean) => {
    if (!open && !saving) {
      setForm({ current: "", newPw: "", repeatPw: "" });
      setError(null);
      onClose();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-black border-4 border-primary box-shadow-retro rounded-none max-w-sm p-0">
        <DialogHeader className="border-b-4 border-primary px-6 py-4">
          <DialogTitle className="font-['Press_Start_2P'] text-primary text-xs text-shadow-retro">
            CHANGE PASSWORD
          </DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-4 px-6 py-5">
          {[
            { label: "CURRENT PASSWORD", key: "current" as const },
            { label: "NEW PASSWORD", key: "newPw" as const },
            { label: "REPEAT NEW PASSWORD", key: "repeatPw" as const },
          ].map(({ label, key }) => (
            <div key={key} className="flex flex-col gap-1">
              <span className="font-['Press_Start_2P'] text-gray-500 text-[9px] tracking-wider">{label}</span>
              <Input
                type="password"
                value={form[key]}
                onChange={e => setForm(prev => ({ ...prev, [key]: e.target.value }))}
                disabled={saving}
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
            disabled={saving}
            className="w-full mt-1"
          >
            {saving ? (
              <span className="flex items-center justify-center gap-2">
                <Loader2 className="w-3 h-3 animate-spin" />
                SAVING...
              </span>
            ) : (
              "SUBMIT"
            )}
          </RetroButton>
        </div>
      </DialogContent>
    </Dialog>
  );
}
