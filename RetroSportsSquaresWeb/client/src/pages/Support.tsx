import { useState } from "react";
import { Link } from "wouter";
import { useAuth } from "@/hooks/use-auth";
import { useSupportRequest } from "@/hooks/use-support";
import { useToast } from "@/hooks/use-toast";
import { RetroButton } from "@/components/RetroButton";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { ArrowLeft, Loader2 } from "lucide-react";
import type { SupportCategory } from "@shared/schema";

const CATEGORIES: { value: SupportCategory; label: string }[] = [
  { value: "Bug", label: "BUG REPORT" },
  { value: "Account", label: "ACCOUNT ISSUE" },
  { value: "Other", label: "OTHER" },
];

const fieldLabelClass = "font-['Press_Start_2P'] text-gray-500 text-[10px] tracking-wider";
const fieldInputClass =
  "border-2 border-primary/30 bg-black text-white font-['VT323'] text-xl rounded-none " +
  "focus-visible:border-primary focus-visible:ring-0 focus-visible:ring-offset-0 placeholder:text-gray-600";

export default function Support() {
  const { user } = useAuth();
  const { toast } = useToast();
  const { mutate: sendSupportRequest, isPending } = useSupportRequest();

  const [name, setName] = useState(user?.displayName ?? "");
  const [email, setEmail] = useState(user?.email ?? "");
  const [category, setCategory] = useState<SupportCategory>("Bug");
  const [message, setMessage] = useState("");
  const [website, setWebsite] = useState(""); // honeypot
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    sendSupportRequest(
      { name: name.trim(), email: email.trim(), category, message: message.trim(), website },
      {
        onSuccess: () => {
          setSubmitted(true);
          setMessage("");
          toast({
            title: "SENT",
            description: "YOUR MESSAGE IS ON ITS WAY.",
            className: "bg-black border-2 border-primary text-primary font-pixel text-[10px]",
          });
        },
        onError: (err) => {
          toast({
            title: "ERROR",
            description: err instanceof Error ? err.message.toUpperCase() : "FAILED TO SEND MESSAGE.",
            variant: "destructive",
            className: "bg-black border-2 border-red-900 text-red-500 font-pixel text-[10px]",
          });
        },
      }
    );
  };

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="border-b-2 border-primary/30 pb-4">
        <h1 className="text-xl md:text-2xl font-['Press_Start_2P'] text-primary text-shadow-retro mb-1">
          SUPPORT
        </h1>
        <p className="font-['VT323'] text-gray-400 text-xl tracking-widest">
          REPORT A BUG OR GET IN TOUCH
        </p>
      </div>

      <div className="border-4 border-primary box-shadow-retro bg-black p-6">
        {submitted ? (
          <div className="py-8 text-center space-y-4">
            <p className="font-['Press_Start_2P'] text-primary text-sm">MESSAGE SENT!</p>
            <p className="font-['VT323'] text-gray-400 text-xl">
              Thanks for reaching out — we'll follow up at {email || "your email"} if needed.
            </p>
            <RetroButton variant="outline" size="sm" onClick={() => setSubmitted(false)}>
              SEND ANOTHER
            </RetroButton>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Honeypot — sr-only clip-hides it (no display:none, which bots check for) without affecting page layout/scroll */}
            <div className="sr-only" aria-hidden="true">
              <label htmlFor="website">Website</label>
              <input
                id="website"
                type="text"
                name="website"
                value={website}
                onChange={(e) => setWebsite(e.target.value)}
                tabIndex={-1}
                autoComplete="off"
              />
            </div>

            <div className="space-y-2">
              <label className={fieldLabelClass}>NAME</label>
              <Input
                required
                value={name}
                maxLength={100}
                onChange={(e) => setName(e.target.value)}
                className={fieldInputClass}
              />
            </div>

            <div className="space-y-2">
              <label className={fieldLabelClass}>EMAIL</label>
              <Input
                required
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className={fieldInputClass}
              />
            </div>

            <div className="space-y-2">
              <label className={fieldLabelClass}>CATEGORY</label>
              <Select value={category} onValueChange={(v) => setCategory(v as SupportCategory)}>
                <SelectTrigger className={fieldInputClass}>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent className="bg-black border-2 border-primary text-white font-['VT323'] text-xl rounded-none">
                  {CATEGORIES.map((c) => (
                    <SelectItem key={c.value} value={c.value} className="focus:bg-primary/20 focus:text-white">
                      {c.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className={fieldLabelClass}>MESSAGE</label>
              <Textarea
                required
                value={message}
                minLength={10}
                maxLength={5000}
                rows={6}
                onChange={(e) => setMessage(e.target.value)}
                placeholder="TELL US WHAT'S GOING ON..."
                className={fieldInputClass}
              />
            </div>

            <RetroButton type="submit" disabled={isPending} className="w-full">
              {isPending ? (
                <span className="flex items-center justify-center gap-2">
                  <Loader2 className="w-3 h-3 animate-spin" /> SENDING...
                </span>
              ) : (
                "SEND MESSAGE"
              )}
            </RetroButton>
          </form>
        )}
      </div>

      <div>
        <Link href="/">
          <RetroButton variant="outline" size="sm">
            <span className="flex items-center gap-2">
              <ArrowLeft className="w-3 h-3" />
              BACK HOME
            </span>
          </RetroButton>
        </Link>
      </div>
    </div>
  );
}
