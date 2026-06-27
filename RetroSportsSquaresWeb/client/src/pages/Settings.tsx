import { useState } from "react";
import { Link, useLocation } from "wouter";
import { useAuth } from "@/hooks/use-auth";
import { RetroButton } from "@/components/RetroButton";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import { PasswordChangeModal } from "@/components/PasswordChangeModal";
import { Pencil, Check, Loader2, ArrowLeft } from "lucide-react";

type EditableFieldProps = {
  label: string;
  value: string;
  inputType?: string;
  onSave: (value: string) => Promise<void>;
};

function EditableField({ label, value, inputType = "text", onSave }: EditableFieldProps) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(value);
  const [saving, setSaving] = useState(false);

  const handleSave = async () => {
    setSaving(true);
    try {
      await onSave(draft);
      setEditing(false);
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    setDraft(value);
    setEditing(false);
  };

  return (
    <div className="flex flex-col gap-1 py-3 border-b border-primary/10 last:border-0">
      <span className="font-['Press_Start_2P'] text-gray-500 text-[9px] tracking-wider">{label}</span>
      <div className="flex items-center gap-2">
        <Input
          type={inputType}
          value={draft}
          disabled={!editing}
          onChange={e => setDraft(e.target.value)}
          className="flex-1 border-2 border-primary/30 bg-black text-white font-['VT323'] text-xl rounded-none
            focus-visible:border-primary focus-visible:ring-0 focus-visible:ring-offset-0
            disabled:opacity-60 disabled:cursor-default placeholder:text-gray-600"
        />
        {editing ? (
          <div className="flex gap-1 shrink-0">
            <button
              onClick={handleCancel}
              disabled={saving}
              className="border-2 border-primary/30 bg-black text-primary/50 hover:text-primary hover:border-primary p-2 transition-colors disabled:opacity-40"
            >
              <span className="font-['Press_Start_2P'] text-[8px]">✕</span>
            </button>
            <button
              onClick={handleSave}
              disabled={saving}
              className="border-2 border-primary bg-primary/10 text-primary hover:bg-primary hover:text-black p-2 transition-colors disabled:opacity-40"
            >
              {saving ? (
                <Loader2 className="w-3 h-3 animate-spin" />
              ) : (
                <Check className="w-3 h-3" />
              )}
            </button>
          </div>
        ) : (
          <button
            onClick={() => setEditing(true)}
            className="border-2 border-primary/30 bg-black text-primary/50 hover:text-primary hover:border-primary p-2 transition-colors shrink-0"
          >
            <Pencil className="w-3 h-3" />
          </button>
        )}
      </div>
    </div>
  );
}


export default function Settings() {
  const { user } = useAuth();
  const [, setLocation] = useLocation();
  const [pwModalOpen, setPwModalOpen] = useState(false);

  if (!user) {
    setLocation("/login");
    return null;
  }

  // TODO: replace with real API calls
  const handleSave = async (_field: string, _value: string) => {
    await new Promise(res => setTimeout(res, 800));
  };

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="border-b-2 border-primary/30 pb-4">
        <h1 className="text-xl md:text-2xl font-['Press_Start_2P'] text-primary text-shadow-retro mb-1">
          SETTINGS
        </h1>
        <p className="font-['VT323'] text-gray-400 text-xl tracking-widest">
          {user.displayName?.toUpperCase() ?? "UNKNOWN"}
        </p>
      </div>

      {/* Main panel — full width, no left column */}
      <div className="border-4 border-primary box-shadow-retro bg-black">
        <Tabs defaultValue="account" className="flex flex-col">

          <TabsList className="w-full bg-black border-b-4 border-primary rounded-none p-0 h-auto shrink-0">
            <TabsTrigger
              value="account"
              className="flex-1 rounded-none py-3 font-['Press_Start_2P'] text-[10px] text-primary border-r-2 border-primary/40
                data-[state=active]:bg-primary data-[state=active]:text-black data-[state=active]:shadow-none"
            >
              ACCOUNT
            </TabsTrigger>
            <TabsTrigger
              value="security"
              className="flex-1 rounded-none py-3 font-['Press_Start_2P'] text-[10px] text-primary
                data-[state=active]:bg-primary data-[state=active]:text-black data-[state=active]:shadow-none"
            >
              SECURITY & ACCESS
            </TabsTrigger>
          </TabsList>

          {/* Account Settings */}
          <TabsContent value="account" className="p-6 mt-0">
            <div className="max-w-lg space-y-1">
              <EditableField
                label="NAME"
                value={user.displayName ?? ""}
                onSave={val => handleSave("name", val)}
              />
              <EditableField
                label="DISPLAY NAME"
                value={user.displayName ?? ""}
                onSave={val => handleSave("displayName", val)}
              />
            </div>
          </TabsContent>

          {/* Security & Access */}
          <TabsContent value="security" className="p-6 mt-0">
            <div className="max-w-lg space-y-6">
              <div className="space-y-1">
                <EditableField
                  label="EMAIL"
                  value={user.email ?? ""}
                  inputType="email"
                  onSave={val => handleSave("email", val)}
                />
              </div>

              <div className="flex flex-col gap-2 pt-2 border-t-2 border-primary/20">
                <span className="font-['Press_Start_2P'] text-gray-500 text-[9px] tracking-wider">PASSWORD</span>
                <p className="font-['VT323'] text-gray-500 text-lg">
                  CHANGE YOUR ACCOUNT PASSWORD
                </p>
                <div>
                  <RetroButton
                    variant="outline"
                    size="sm"
                    onClick={() => setPwModalOpen(true)}
                  >
                    CHANGE PASSWORD
                  </RetroButton>
                </div>
              </div>
            </div>
          </TabsContent>

        </Tabs>
      </div>

      {/* Back navigation */}
      <div>
        <Link href="/player-dashboard">
          <RetroButton variant="outline" size="sm">
            <span className="flex items-center gap-2">
              <ArrowLeft className="w-3 h-3" />
              BACK TO DASHBOARD
            </span>
          </RetroButton>
        </Link>
      </div>

      <PasswordChangeModal open={pwModalOpen} onClose={() => setPwModalOpen(false)} />
    </div>
  );
}
