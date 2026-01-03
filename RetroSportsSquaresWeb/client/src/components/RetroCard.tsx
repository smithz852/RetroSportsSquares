import { ReactNode } from "react";
import { cn } from "@/lib/utils";

interface RetroCardProps {
  children: ReactNode;
  className?: string;
  title?: string;
}

export function RetroCard({ children, className, title }: RetroCardProps) {
  return (
    <div className={cn(
      "border-4 border-primary bg-black p-6 relative",
      "box-shadow-retro",
      className
    )}>
      {title && (
        <div className="absolute -top-5 left-4 bg-black px-2 border-2 border-primary text-primary font-['Press_Start_2P'] text-xs py-1">
          {title}
        </div>
      )}
      {children}
    </div>
  );
}
