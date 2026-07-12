import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Loader2 } from "lucide-react";
import { format } from "date-fns";
import { useAdminGameChatLog } from "@/hooks/use-admin-dashboard";

interface AdminChatLogDialogProps {
  gameId: string | null;
  gameName: string;
  onClose: () => void;
}

export function AdminChatLogDialog({ gameId, gameName, onClose }: AdminChatLogDialogProps) {
  const { data: messages, isLoading, error } = useAdminGameChatLog(gameId);

  return (
    <Dialog open={!!gameId} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="bg-black border-4 border-primary rounded-none max-w-lg">
        <DialogHeader>
          <DialogTitle className="font-['Press_Start_2P'] text-primary text-sm uppercase">
            CHAT LOG
          </DialogTitle>
          <DialogDescription className="font-['VT323'] text-gray-400 text-xl">
            {gameName}
          </DialogDescription>
        </DialogHeader>
        <div className="max-h-[400px] overflow-y-auto custom-scrollbar space-y-2 pr-1">
          {isLoading ? (
            <div className="flex items-center justify-center gap-3 py-8">
              <Loader2 className="h-5 w-5 text-primary animate-spin" />
              <span className="font-['Press_Start_2P'] text-primary text-xs">LOADING...</span>
            </div>
          ) : error ? (
            <p className="font-['Press_Start_2P'] text-primary text-xs text-center py-8">
              {error.message.toUpperCase()}
            </p>
          ) : !messages?.length ? (
            <p className="font-['Press_Start_2P'] text-gray-600 text-xs text-center py-8">
              NO MESSAGES
            </p>
          ) : (
            messages.map((m) => (
              <div key={m.id} className="p-2 border border-primary/20">
                <div className="flex items-baseline justify-between gap-2">
                  <span className="font-['Press_Start_2P'] text-[9px] text-primary truncate">
                    {m.displayName}
                  </span>
                  <span className="font-['VT323'] text-sm text-gray-500 shrink-0">
                    {format(new Date(m.createdAt), "MMM dd HH:mm")}
                  </span>
                </div>
                <p
                  className={`font-['VT323'] text-lg leading-tight break-words ${
                    m.isDeleted ? "text-gray-600 line-through" : "text-gray-300"
                  }`}
                >
                  {m.message}
                </p>
                {m.isDeleted && (
                  <span className="font-['Press_Start_2P'] text-[8px] px-1.5 py-0.5 bg-red-900 text-red-400 border border-red-600">
                    DELETED
                  </span>
                )}
              </div>
            ))
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
