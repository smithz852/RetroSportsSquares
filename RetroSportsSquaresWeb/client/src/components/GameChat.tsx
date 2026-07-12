import { useState, useEffect, useRef } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { format } from "date-fns";
import { useToast } from "@/hooks/use-toast";
import { useChatMessages, useSendChatMessage } from "@/hooks/use-chat";

const MAX_MESSAGE_LENGTH = 500;

interface GameChatProps {
  gameId: string;
  currentUserId: string;
  canSend: boolean;
}

export function GameChat({ gameId, currentUserId, canSend }: GameChatProps) {
  const { toast } = useToast();
  const { data: messages } = useChatMessages(gameId);
  const { mutate: sendMessage, isPending } = useSendChatMessage(gameId);
  const [draft, setDraft] = useState("");
  const listRef = useRef<HTMLDivElement | null>(null);

  // Keep the newest message in view without scrolling the page itself
  useEffect(() => {
    const list = listRef.current;
    if (list) list.scrollTop = list.scrollHeight;
  }, [messages?.length]);

  const handleSend = () => {
    const message = draft.trim();
    if (!message || isPending || !canSend) return;
    sendMessage(message, {
      onSuccess: () => setDraft(""),
      onError: (err: Error) => {
        toast({
          title: "MESSAGE FAILED",
          description: err.message,
          variant: "destructive",
          className: "bg-black border-2 border-red-600 text-red-500 font-['VT323']",
        });
      },
    });
  };

  return (
    <Card className="bg-black border-4 border-red-900 rounded-none shadow-[0_0_20px_rgba(255,0,0,0.1)] flex flex-col flex-1">
      <CardHeader className="border-b-2 border-red-900 p-3">
        <CardTitle className="text-xl text-red-600 font-pixel text-center uppercase tracking-tighter">
          COMMS
        </CardTitle>
      </CardHeader>
      <CardContent className="p-3 flex flex-col flex-1 min-h-0 gap-2">
        <div
          ref={listRef}
          className="flex-1 min-h-[180px] max-h-[400px] overflow-y-auto custom-scrollbar space-y-2 pr-1"
        >
          {!messages || messages.length === 0 ? (
            <p className="text-red-900/40 font-pixel text-xs text-center uppercase pt-4">
              No messages yet
            </p>
          ) : (
            messages.map((m) => {
              const isOwn = m.userId === currentUserId;
              return (
                <div
                  key={m.id}
                  className={`p-2 border ${
                    isOwn ? "border-red-500 bg-red-900/20" : "border-red-900/40"
                  }`}
                >
                  <div className="flex items-baseline justify-between gap-2">
                    <span
                      className={`font-pixel text-[10px] truncate uppercase ${
                        isOwn ? "text-red-400" : "text-red-700"
                      }`}
                    >
                      {m.displayName}
                    </span>
                    <span className="font-['VT323'] text-xs text-red-900/60 shrink-0">
                      {format(new Date(m.createdAt), "HH:mm")}
                    </span>
                  </div>
                  <p className="font-['VT323'] text-lg leading-tight text-red-500 break-words">
                    {m.message}
                  </p>
                </div>
              );
            })
          )}
        </div>
        <div className="space-y-1">
          <div className="flex gap-2">
            <Input
              value={draft}
              onChange={(e) => setDraft(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  handleSend();
                }
              }}
              maxLength={MAX_MESSAGE_LENGTH}
              disabled={!canSend || isPending}
              placeholder={canSend ? "TYPE MESSAGE..." : "SPECTATORS CANNOT CHAT"}
              className="bg-black border-2 border-red-900 rounded-none text-red-500 font-['VT323'] text-lg placeholder:text-red-900/60 focus-visible:ring-red-600"
            />
            <Button
              onClick={handleSend}
              disabled={!canSend || isPending || !draft.trim()}
              className="bg-red-600 hover:bg-red-500 text-black font-pixel text-xs rounded-none px-3"
            >
              SEND
            </Button>
          </div>
          <div className="text-right">
            <span className="font-['VT323'] text-xs text-red-900/60">
              {draft.length}/{MAX_MESSAGE_LENGTH}
            </span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
