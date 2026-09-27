import { useState, type ReactNode } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { RetroButton } from "@/components/RetroButton";

export type InfoModalPage = {
  heading: string;
  body: ReactNode;
};

type Props = {
  open: boolean;
  onClose: () => void;
  title: string;
  pages: InfoModalPage[];
};

export function InfoModal({ open, onClose, title, pages }: Props) {
  const [pageIndex, setPageIndex] = useState(0);
  const page = pages[pageIndex];
  const isFirst = pageIndex === 0;
  const isLast = pageIndex === pages.length - 1;

  const handleOpenChange = (nextOpen: boolean) => {
    if (!nextOpen) {
      onClose();
      setPageIndex(0);
    }
  };

  if (!page) return null;

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="bg-black border-4 border-primary box-shadow-retro rounded-none max-w-lg p-0 max-h-[85vh] flex flex-col gap-0">
        <DialogHeader className="border-b-4 border-primary px-6 py-4 shrink-0">
          <DialogTitle className="font-['Press_Start_2P'] text-primary text-xs text-shadow-retro">
            {title}
          </DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-4 px-6 py-5 overflow-y-auto custom-scrollbar">
          <h3 className="font-['Press_Start_2P'] text-red-500 text-sm leading-relaxed">
            {page.heading}
          </h3>
          <div className="font-['VT323'] text-white text-xl leading-tight space-y-3">
            {page.body}
          </div>
        </div>

        <div className="flex items-center justify-between gap-4 border-t-4 border-primary px-6 py-4 shrink-0">
          {pages.length > 1 ? (
            <div className="flex gap-1.5">
              {pages.map((_, i) => (
                <span
                  key={i}
                  className={`w-2 h-2 rounded-full ${i === pageIndex ? "bg-primary" : "bg-primary/20"}`}
                />
              ))}
            </div>
          ) : (
            <span />
          )}

          <div className="flex gap-3">
            {!isFirst && (
              <RetroButton variant="outline" size="sm" onClick={() => setPageIndex(i => i - 1)}>
                BACK
              </RetroButton>
            )}
            <RetroButton
              variant="primary"
              size="sm"
              onClick={() => (isLast ? handleOpenChange(false) : setPageIndex(i => i + 1))}
            >
              {isLast ? "GOT IT" : "NEXT"}
            </RetroButton>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
